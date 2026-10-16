using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Events;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Events;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Parties.Events;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims;

public sealed class ClaimTests
{
    private static readonly StateChangeReason _reason = new("NEW_EVIDENCE", "Late evidence arrived");
    private static readonly RiskObjectDetails _van = new(AssetType.Vehicle, "Van");
    private static readonly LossDetails _loss = TestClaims.Loss(TestClaims.Start.AddDays(-2), TestClaims.Description);

    public static TheoryData<string, Func<Claim>, Action<Claim, LossReference>> Edits => new()
    {
        { "notes", Complete, UpdateNotes },
        { "handler", Complete, AssignHandler },
        { "severity", Complete, SetSeverity },
        { "policy", Complete, LinkPolicy },
        { "override", Complete, SetOverride },
        { "add party", Complete, AddParty },
        { "remove party", WithWitness, RemoveParty },
        { "add risk object", Complete, AddRiskObject },
        { "update risk object", Complete, UpdateRiskObject },
        { "remove risk object", Complete, RemoveRiskObject },
        { "loss", Complete, UpdateLoss },
        { "waive", WithShortDescription, WaiveIssue },
        { "acknowledge", WithPolicyWarning, AcknowledgeIssue },
    };

    public static TheoryData<string, Action<Claim, LossReference>> Moves => new()
    {
        { "investigate", Investigate },
        { "send to payment", SendToPayment },
    };

    [Fact]
    [Trait("Rule", "S:INTAKE-NoPolicy")]
    public void Create_ReturnsADraftWithItsIssuesAlreadyEvaluated()
    {
        var input = CreateInput();

        var outcome = Claim.Create(input);

        outcome.Value.State.Should().BeOfType<ClaimDraftState>();
        outcome.Value.Issues.Select(issue => issue.Finding.Code)
            .Should().Contain([IsLossDatePresent.Code, HasLinkedPolicy.Code]);
    }

    [Fact]
    [Trait("Rule", HasClaimant.Code)]
    public void AddParty_AClaimant_ClearsTheClaimantIssueInTheSameCall()
    {
        var claim = TestClaims.Create();
        var input = new AddPartyInput(TestClaims.Claimant(), TestClaims.LossReference());

        claim.AddParty(input);

        var issue = claim.Issues.Single(issue => issue.Finding.HasCode(HasClaimant.Code));
        issue.State.Should().BeOfType<IssueClearedState>();
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void AddParty_ToAClosedClaim_ReturnsTheFindingAndEvaluatesNothing()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var issuesBefore = claim.Issues.Count;
        var input = new AddPartyInput(TestClaims.Claimant(), TestClaims.LossReference());

        var outcome = claim.AddParty(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
        claim.Issues.Should().HaveCount(issuesBefore);
    }

    [Fact]
    [Trait("Rule", "S:ST-Reopened-Open")]
    public void Reopen_BySupervisor_EndsInOpenWithTwoStateChanges()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(TestClaims.Supervisor, TestClaims.LossReference())
        {
            Reason = _reason,
        };

        var outcome = claim.Reopen(input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
        claim.StateChanges.TakeLast(2).Select(change => change.To.Name).Should().Equal("Reopened", "Open");
    }

    [Fact]
    public void Create_RaisesClaimCreated()
    {
        var input = CreateInput();

        var outcome = Claim.Create(input);

        var created = outcome.Value.PopEvents().OfType<ClaimCreatedEvent>().Should().ContainSingle().Subject;
        created.ClaimId.Should().Be(outcome.Value.Id);
        created.ClaimNumber.Should().Be("CLM-2026-0000001");
    }

    [Fact]
    public void AddParty_RaisesPartyAdded()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new AddPartyInput(TestClaims.Witness(), TestClaims.LossReference());

        var outcome = claim.AddParty(input);

        var added = claim.PopEvents().OfType<PartyAddedEvent>().Should().ContainSingle().Subject;
        added.PartyId.Should().Be(outcome.Value.Id);
    }

    [Fact]
    public void Open_RaisesOneStateChangedEventWithBothStates()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new ClaimTransitionInput(TestClaims.LossReference());

        claim.Open(input);

        var changed = claim.PopEvents().OfType<ClaimStateChangedEvent>().Should().ContainSingle().Subject;
        changed.ClaimId.Should().Be(claim.Id);
        changed.From.Should().Be("Draft");
        changed.To.Should().Be("Open");
    }

    [Fact]
    [Trait("Rule", "S:ST-Open-Withdrawn")]
    public void Withdraw_RaisesTheReasonOnTheStateChangedEvent()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var reason = new StateChangeReason("CUSTOMER_REQUEST", "The insured withdrew the claim");
        var input = new FreezeClaimInput(false, false, false, TestClaims.LossReference())
        {
            Reason = reason,
        };

        claim.Withdraw(input);

        var changed = claim.PopEvents().OfType<ClaimStateChangedEvent>().Should().ContainSingle().Subject;
        changed.Reason.Should().Be(reason);
    }

    [Fact]
    [Trait("Rule", "S:ST-Reopened-Open")]
    public void Reopen_RaisesAStateChangedEventForEachMove()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(TestClaims.Supervisor, TestClaims.LossReference())
        {
            Reason = _reason,
        };

        claim.Reopen(input);

        var events = claim.PopEvents().OfType<ClaimStateChangedEvent>().ToList();
        events.TakeLast(2).Select(changed => changed.To).Should().Equal("Reopened", "Open");
    }

    [Fact]
    public void Create_RaisesAnIssueRaisedEventForEveryIssue()
    {
        var input = CreateInput();

        var outcome = Claim.Create(input);

        var claim = outcome.Value;
        var raised = claim.PopEvents().OfType<ValidationIssueRaisedEvent>().ToList();
        raised.Select(issue => issue.Code).Should().BeEquivalentTo(claim.Issues.Select(issue => issue.Finding.Code));
        raised.Should().OnlyContain(issue => issue.ClaimId == claim.Id);
    }

    [Fact]
    public void EvaluateIssues_WhenNothingChanges_RaisesNothing()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new EvaluateIssuesInput(TestClaims.LossReference());

        var outcome = claim.EvaluateIssues(input);

        outcome.Value.Raised.Should().BeEmpty();
        outcome.Value.Cleared.Should().BeEmpty();
        claim.PopEvents().Should().BeEmpty();
    }

    [Fact]
    public void SetSeverity_WhenNoIssueChanges_RaisesNoIssueEvents()
    {
        var claim = TestClaims.Empty();
        var currentBefore = claim.Issues.Count(issue => issue.State.IsCurrent);
        claim.PopEvents();
        var input = new SetSeverityInput(ClaimSeverity.Critical, TestClaims.LossReference());

        claim.SetSeverity(input);

        claim.PopEvents().OfType<ValidationIssueRaisedEvent>().Should().BeEmpty();
        claim.Issues.Count(issue => issue.State.IsCurrent).Should().Be(currentBefore);
    }

    [Fact]
    public void HasActiveClaimant_WithoutAClaimant_IsFalse()
    {
        var claim = TestClaims.Create(TestClaims.Witness());

        claim.HasActiveClaimant.Should().BeFalse();
    }

    [Fact]
    public void HasActiveClaimant_WithAClaimant_IsTrue()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        claim.HasActiveClaimant.Should().BeTrue();
    }

    [Fact]
    public void HasUnresolvedCriticalIssues_ForAnEmptyClaim_IsTrue()
    {
        var claim = TestClaims.Empty();

        claim.HasUnresolvedCriticalIssues.Should().BeTrue();
    }

    [Fact]
    public void HasUnresolvedCriticalIssues_WhenTheOnlyCriticalIssueIsWaived_IsFalse()
    {
        var claim = TestClaims.WithDescription("Leak");
        TestClaims.WaiveIssue(claim, IsDescriptionLongEnough.Code);

        claim.HasUnresolvedCriticalIssues.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(Edits))]
    public void Edit_ReevaluatesTheIssuesWithTheInputsReference(
        string edit,
        Func<Claim> createClaim,
        Action<Claim, LossReference> change)
    {
        var claim = createClaim();
        var reference = TestClaims.ReferenceWithCauseCodes("OTHER");

        change(claim, reference);

        claim.Issues.Should().Contain(
            issue => issue.State.IsCurrent && issue.Finding.HasCode(IsCauseCodeActive.Code),
            "the {0} edit settles the claim",
            edit);
    }

    [Theory]
    [MemberData(nameof(Moves))]
    public void Move_RecordsTheMoveAndReevaluatesTheIssues(string move, Action<Claim, LossReference> change)
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var reference = TestClaims.ReferenceWithCauseCodes("OTHER");

        change(claim, reference);

        claim.PopEvents().OfType<ClaimStateChangedEvent>().Should().ContainSingle("the {0} move is recorded", move);
        claim.Issues.Should().Contain(issue => issue.State.IsCurrent && issue.Finding.HasCode(IsCauseCodeActive.Code));
    }

    [Fact]
    public void HasActiveRiskObject_WithARiskObject_IsTrue()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        claim.HasActiveRiskObject.Should().BeTrue();
    }

    [Fact]
    public void HasActiveRiskObject_AfterTheOnlyRiskObjectIsRemoved_IsFalse()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var remove = new RemoveRiskObjectInput(claim.RiskObjects[0].Id, TestClaims.Later, TestClaims.LossReference());
        claim.RemoveRiskObject(remove);

        claim.HasActiveRiskObject.Should().BeFalse();
    }

    [Fact]
    public void IsOnlyActiveClaimant_ForTheOnlyClaimant_IsTrue()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());

        var result = claim.IsOnlyActiveClaimant(claim.Parties[0].Id);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsOnlyActiveClaimant_ForAWitnessBesideOneClaimant_IsFalse()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());

        var result = claim.IsOnlyActiveClaimant(claim.Parties[1].Id);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsOnlyActiveClaimant_WhenAnotherClaimantRemains_IsFalse()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Claimant());

        var result = claim.IsOnlyActiveClaimant(claim.Parties[0].Id);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsOnlyActiveClaimant_ForAnAlreadyRemovedClaimant_IsFalse()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Claimant());
        var removed = claim.Parties[0].Id;
        claim.RemoveParty(new RemovePartyInput(removed, TestClaims.LossReference()));

        var result = claim.IsOnlyActiveClaimant(removed);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasOtherPrimaryRiskObject_WhenNoneIsPrimary_IsFalse()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = claim.HasOtherPrimaryRiskObject(Guid.Empty);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasOtherPrimaryRiskObject_WhenAnotherIsPrimary_IsTrue()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        TestClaims.AddPrimaryRiskObject(claim);

        var result = claim.HasOtherPrimaryRiskObject(Guid.Empty);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasOtherPrimaryRiskObject_WhenTheOnlyPrimaryIsTheOneAsked_IsFalse()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var primary = TestClaims.AddPrimaryRiskObject(claim);

        var result = claim.HasOtherPrimaryRiskObject(primary.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasOtherPrimaryRiskObject_IgnoresARemovedPrimary()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var primary = TestClaims.AddPrimaryRiskObject(claim);
        claim.RemoveRiskObject(new RemoveRiskObjectInput(primary.Id, TestClaims.Later, TestClaims.LossReference()));

        var result = claim.HasOtherPrimaryRiskObject(Guid.Empty);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasUnresolvedWarning_WhenTheWarningIsAbsent_IsFalse()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = claim.HasUnresolvedWarning(IsLossDateWithinPolicyPeriod.Code);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasUnresolvedWarning_WhenTheWarningIsUnresolved_IsTrue()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);

        var result = claim.HasUnresolvedWarning(IsLossDateWithinPolicyPeriod.Code);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasUnresolvedWarning_WhenTheWarningIsAcknowledged_IsFalse()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        TestClaims.AcknowledgeIssue(claim, IsLossDateWithinPolicyPeriod.Code);

        var result = claim.HasUnresolvedWarning(IsLossDateWithinPolicyPeriod.Code);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasUnresolvedWarning_WhenOnlyAnotherWarningIsUnresolved_IsFalse()
    {
        var claim = TestClaims.WithoutPolicy();

        var result = claim.HasUnresolvedWarning(IsLossDateWithinPolicyPeriod.Code);

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", HasNoCriticalIssuesForOpen.Code)]
    public void Open_FromDraftWhenItsReferenceDeactivatesTheCauseCode_IsRefusedByTheFreshIssue()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new ClaimTransitionInput(TestClaims.ReferenceWithCauseCodes("OTHER"));

        var outcome = claim.Open(input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Contain(HasNoCriticalIssuesForOpen.Code);
        claim.State.Should().BeOfType<ClaimDraftState>();
    }

    [Fact]
    [Trait("Rule", HasNoCriticalIssuesForClose.Code)]
    public void Close_WhenItsReferenceDeactivatesTheCauseCode_IsRefusedByTheFreshIssue()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = new FreezeClaimInput(false, false, false, TestClaims.ReferenceWithCauseCodes("OTHER"));

        var outcome = claim.Close(input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Contain(HasNoCriticalIssuesForClose.Code);
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    public void Withdraw_RecordsTheIssuesOfItsReferenceBeforeTheClaimFreezes()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = WithdrawInput(TestClaims.ReferenceWithCauseCodes("OTHER"));

        claim.Withdraw(input);

        claim.State.Should().BeOfType<ClaimWithdrawnState>();
        claim.Issues.Should().Contain(issue => issue.State.IsCurrent && issue.Finding.HasCode(IsCauseCodeActive.Code));
    }

    [Fact]
    public void UpdateNotes_OnAClosedClaim_KeepsTheIssues()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var reference = TestClaims.ReferenceWithCauseCodes("OTHER");
        var input = new UpdateNotesInput("Late call", reference);

        claim.UpdateNotes(input);

        claim.Issues.Should().NotContain(
            issue => issue.State.IsCurrent && issue.Finding.HasCode(IsCauseCodeActive.Code));
    }

    [Theory]
    [Trait("Rule", "S:BR-R-05")]
    [InlineData(10_000_000, true)]
    [InlineData(10_000_001, false)]
    public void IsWithinReserveLimit_IncludesTheLimitItself(int reserveTotal, bool isExpected)
    {
        var result = Claim.IsWithinReserveLimit(reserveTotal);

        result.Should().Be(isExpected);
    }

    [Fact]
    public void HasAssignedHandler_WithoutAHandler_IsFalse()
    {
        var claim = TestClaims.Empty();

        claim.HasAssignedHandler.Should().BeFalse();
    }

    [Fact]
    public void HasAssignedHandler_WithAHandler_IsTrue()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        claim.HasAssignedHandler.Should().BeTrue();
    }

    [Theory]
    [Trait("Rule", "S:BR-R-05")]
    [InlineData(9_990_000, 10_000, true)]
    [InlineData(9_990_000, 10_001, false)]
    [InlineData(12_000_000, -1_000, true)]
    public void IsReserveChangeWithinLimit_PassesAnyChangeThatDoesNotIncrease(
        int approvedTotal,
        int amount,
        bool isExpected)
    {
        var result = Claim.IsReserveChangeWithinLimit(approvedTotal, amount);

        result.Should().Be(isExpected);
    }

    [Fact]
    public void HasLinkedPolicy_WithoutAPolicy_IsFalse()
    {
        var claim = TestClaims.WithoutPolicy();

        claim.HasLinkedPolicy.Should().BeFalse();
    }

    [Fact]
    public void HasLinkedPolicy_WithAPolicy_IsTrue()
    {
        var claim = TestClaims.Empty();

        claim.HasLinkedPolicy.Should().BeTrue();
    }

    [Fact]
    public void HasWaivedFinding_ForAWaivedIssueWithTheSameEvidence_IsTrue()
    {
        var claim = TestClaims.WithDescription("Leak");
        TestClaims.WaiveIssue(claim, IsDescriptionLongEnough.Code);
        var finding = claim.Issues.Single(issue => issue.State.IsWaived).Finding;

        var result = claim.HasWaivedFinding(finding);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasWaivedFinding_ForTheSameCodeWithOtherEvidence_IsFalse()
    {
        var claim = TestClaims.WithDescription("Leak");
        TestClaims.WaiveIssue(claim, IsDescriptionLongEnough.Code);
        var waived = claim.Issues.Single(issue => issue.State.IsWaived).Finding;
        var other = waived with { Evidence = "Drip" };

        var result = claim.HasWaivedFinding(other);

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:BR-R-05")]
    public void SetLimitOverride_RaisesTheOverrideChangedEvent()
    {
        var claim = TestClaims.Empty();
        claim.PopEvents();
        var input = new SetLimitOverrideInput(true, TestClaims.Manager, 0m, TestClaims.LossReference());

        claim.SetLimitOverride(input);

        claim.PopEvents().OfType<AggregateLimitOverrideChangedEvent>().Should().ContainSingle()
            .Which.HasAggregateLimitOverride.Should().BeTrue();
    }

    private static CreateClaimInput CreateInput()
    {
        var input = new CreateClaimInput(
            TestClaims.Organisation,
            ClaimNumber.From("CLM-2026-0000001"),
            TestClaims.Start,
            TestClaims.Start,
            TestClaims.NoPolicyReference());

        return input;
    }

    private static Claim Complete()
    {
        return TestClaims.Create(TestClaims.Claimant());
    }

    private static Claim WithWitness()
    {
        return TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
    }

    private static Claim WithShortDescription()
    {
        return TestClaims.WithDescription("Leak");
    }

    private static Claim WithPolicyWarning()
    {
        return TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
    }

    private static Guid CurrentIssueId(Claim claim, string code)
    {
        var issue = claim.Issues.Single(issue => issue.State.IsCurrent && issue.Finding.HasCode(code));

        return issue.Id;
    }

    private static SetLimitOverrideInput OverrideInput(LossReference reference)
    {
        return new SetLimitOverrideInput(true, TestClaims.Manager, 0m, reference);
    }

    private static RemovePartyInput RemoveWitnessInput(Claim claim, LossReference reference)
    {
        return new RemovePartyInput(claim.Parties[1].Id, reference);
    }

    private static UpdateRiskObjectInput UpdateRiskObjectInput(Claim claim, LossReference reference)
    {
        return new UpdateRiskObjectInput(claim.RiskObjects[0].Id, _van, reference);
    }

    private static RemoveRiskObjectInput RemoveRiskObjectInput(Claim claim, LossReference reference)
    {
        return new RemoveRiskObjectInput(claim.RiskObjects[0].Id, TestClaims.Later, reference);
    }

    private static WaiveIssueInput WaiveInput(Claim claim, LossReference reference)
    {
        var issueId = CurrentIssueId(claim, IsDescriptionLongEnough.Code);

        return new WaiveIssueInput(issueId, TestClaims.Supervisor, TestClaims.Actor, TestClaims.Later, reference);
    }

    private static AcknowledgeIssueInput AcknowledgeInput(Claim claim, LossReference reference)
    {
        var issueId = CurrentIssueId(claim, IsLossDateWithinPolicyPeriod.Code);

        return new AcknowledgeIssueInput(issueId, TestClaims.Actor, TestClaims.Later, reference);
    }

    private static SendToPaymentClaimInput SendToPaymentInput(LossReference reference)
    {
        return new SendToPaymentClaimInput(true, reference);
    }

    private static FreezeClaimInput WithdrawInput(LossReference reference)
    {
        return new FreezeClaimInput(false, false, false, reference) { Reason = _reason };
    }

    private static void UpdateNotes(Claim claim, LossReference reference)
    {
        claim.UpdateNotes(new UpdateNotesInput("Called", reference));
    }

    private static void AssignHandler(Claim claim, LossReference reference)
    {
        claim.AssignHandler(new AssignHandlerInput(TestClaims.Actor, reference));
    }

    private static void SetSeverity(Claim claim, LossReference reference)
    {
        claim.SetSeverity(new SetSeverityInput(ClaimSeverity.Minor, reference));
    }

    private static void LinkPolicy(Claim claim, LossReference reference)
    {
        claim.LinkPolicy(new LinkPolicyInput(TestClaims.Policy, reference));
    }

    private static void SetOverride(Claim claim, LossReference reference)
    {
        claim.SetLimitOverride(OverrideInput(reference));
    }

    private static void AddParty(Claim claim, LossReference reference)
    {
        claim.AddParty(new AddPartyInput(TestClaims.Witness(), reference));
    }

    private static void RemoveParty(Claim claim, LossReference reference)
    {
        claim.RemoveParty(RemoveWitnessInput(claim, reference));
    }

    private static void AddRiskObject(Claim claim, LossReference reference)
    {
        claim.AddRiskObject(new AddRiskObjectInput(_van, reference));
    }

    private static void UpdateRiskObject(Claim claim, LossReference reference)
    {
        claim.UpdateRiskObject(UpdateRiskObjectInput(claim, reference));
    }

    private static void RemoveRiskObject(Claim claim, LossReference reference)
    {
        claim.RemoveRiskObject(RemoveRiskObjectInput(claim, reference));
    }

    private static void UpdateLoss(Claim claim, LossReference reference)
    {
        claim.UpdateLossDetails(new UpdateLossDetailsInput(_loss, reference));
    }

    private static void WaiveIssue(Claim claim, LossReference reference)
    {
        claim.WaiveIssue(WaiveInput(claim, reference));
    }

    private static void AcknowledgeIssue(Claim claim, LossReference reference)
    {
        claim.AcknowledgeIssue(AcknowledgeInput(claim, reference));
    }

    private static void Investigate(Claim claim, LossReference reference)
    {
        claim.Investigate(new ClaimTransitionInput(reference));
    }

    private static void SendToPayment(Claim claim, LossReference reference)
    {
        claim.SendToPayment(SendToPaymentInput(reference));
    }

    private static void Close(Claim claim, LossReference reference)
    {
        claim.Close(new FreezeClaimInput(false, false, false, reference));
    }

    private static void Withdraw(Claim claim, LossReference reference)
    {
        claim.Withdraw(WithdrawInput(reference));
    }
}
