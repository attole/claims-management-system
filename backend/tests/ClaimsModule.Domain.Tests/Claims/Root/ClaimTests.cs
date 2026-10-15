using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Issues.Events;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Checks;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Parties.Events;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Events;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root;

public sealed class ClaimTests
{
    private static readonly StateChangeReason _reason = new("NEW_EVIDENCE", "Late evidence arrived");

    [Fact]
    [Trait("Rule", "S:INTAKE-NoPolicy")]
    public void Create_ReturnsADraftWithItsIssuesAlreadyEvaluated()
    {
        var input = new CreateClaimInput(
            TestClaims.Organisation,
            ClaimNumber.From("CLM-2026-0000001"),
            TestClaims.Actor,
            TestClaims.Start,
            TestClaims.Reference());

        var outcome = Claim.Create(input);

        outcome.Value.State.Should().BeOfType<ClaimDraftState>();
        outcome.Value.Issues.Select(issue => issue.Finding.Code)
            .Should().Contain([IsLossDatePresent.Code, HasLinkedPolicy.Code]);
    }

    [Fact]
    [Trait("Rule", HasOrganisation.Code)]
    public void Create_WithoutAnOrganisation_ReturnsTheFindingAndNoClaim()
    {
        var input = new CreateClaimInput(
            Guid.Empty,
            ClaimNumber.From("CLM-2026-0000001"),
            TestClaims.Actor,
            TestClaims.Start,
            TestClaims.Reference());

        var outcome = Claim.Create(input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasOrganisation.Code);
    }

    [Fact]
    [Trait("Rule", HasClaimant.Code)]
    public void AddParty_AClaimant_ClearsTheClaimantIssueInTheSameCall()
    {
        var claim = TestClaims.Create();
        var input = new AddPartyInput(
            TestClaims.Claimant(),
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        claim.AddParty(input);

        var issue = claim.Issues.Single(candidate => candidate.Finding.Code == HasClaimant.Code);
        issue.State.Should().BeOfType<IssueClearedState>();
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void AddParty_ToAClosedClaim_ReturnsTheFindingAndEvaluatesNothing()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var issuesBefore = claim.Issues.Count;
        var input = new AddPartyInput(
            TestClaims.Claimant(),
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = claim.AddParty(input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
        claim.Issues.Should().HaveCount(issuesBefore);
    }

    [Fact]
    [Trait("Rule", "S:ST-Reopened-Open")]
    public void Reopen_BySupervisor_EndsInOpenWithTwoStateChanges()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(
            TestClaims.Supervisor,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference())
        {
            Reason = _reason,
        };

        var outcome = claim.Reopen(input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
        claim.StateChanges.TakeLast(2).Select(change => change.To.Name).Should().Equal("Reopened", "Open");
    }

    [Fact]
    [Trait("Rule", HasPermittedRole.Code)]
    public void Reopen_ByAHandler_ReturnsTheFindingAndStaysClosed()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later, TestClaims.Reference())
        {
            Reason = _reason,
        };

        var outcome = claim.Reopen(input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasPermittedRole.Code);
        claim.State.Should().BeOfType<ClaimClosedState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-Draft-Open")]
    public void Open_FromADraft_OpensTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new OpenClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = claim.Open(input);

        outcome.Value.To.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    public void Create_RaisesClaimCreated()
    {
        var input = new CreateClaimInput(
            TestClaims.Organisation,
            ClaimNumber.From("CLM-2026-0000001"),
            TestClaims.Actor,
            TestClaims.Start,
            TestClaims.Reference());

        var outcome = Claim.Create(input);

        var created = outcome.Value.PopEvents().OfType<ClaimCreatedEvent>().Should().ContainSingle().Subject;
        created.ClaimId.Should().Be(outcome.Value.Id);
        created.ClaimNumber.Should().Be("CLM-2026-0000001");
    }

    [Fact]
    public void AddParty_TouchesTheClaimAndRaisesPartyAdded()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new AddPartyInput(TestClaims.Witness(), TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = claim.AddParty(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
        var added = claim.PopEvents().OfType<PartyAddedEvent>().Should().ContainSingle().Subject;
        added.PartyId.Should().Be(outcome.Value.Id);
    }

    [Fact]
    public void RemoveParty_TouchesTheClaimAndRaisesPartyRemoved()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        claim.PopEvents();
        var witness = claim.Parties[1];
        var input = new RemovePartyInput(witness.Id, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        claim.RemoveParty(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
        claim.PopEvents().OfType<PartyRemovedEvent>().Should().ContainSingle().Which.PartyId.Should().Be(witness.Id);
    }

    [Fact]
    public void AddRiskObject_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var van = new RiskObjectDetails(AssetType.Vehicle, "Van");
        var input = new AddRiskObjectInput(van, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        claim.AddRiskObject(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void RemoveRiskObject_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObjectId = claim.RiskObjects[0].Id;
        var input = new RemoveRiskObjectInput(riskObjectId, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        claim.RemoveRiskObject(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void UpdateLossDetails_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-2), TestClaims.Description);
        var input = new UpdateLossDetailsInput(loss, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        claim.UpdateLossDetails(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void AssignHandler_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new AssignHandlerInput(Guid.NewGuid(), TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        claim.AssignHandler(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void AssignHandler_WhenTheClaimIsWithdrawn_LeavesTheClaimUntouched()
    {
        var claim = TestClaims.InState<ClaimWithdrawnState>();
        var updatedAt = claim.UpdatedAt;
        var input = new AssignHandlerInput(Guid.NewGuid(), TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        claim.AssignHandler(input);

        claim.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void SetSeverity_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SetSeverityInput(
            ClaimSeverity.Critical,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        claim.SetSeverity(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void UpdateNotes_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new UpdateNotesInput(
            "called the insured",
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        claim.UpdateNotes(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void LinkPolicy_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new LinkPolicyInput(Guid.NewGuid(), TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        claim.LinkPolicy(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void SetLimitOverride_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SetLimitOverrideInput(
            true,
            TestClaims.Manager,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        claim.SetLimitOverride(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void WaiveIssue_RaisesAWaivedEventForTheIssue()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues[0];
        claim.PopEvents();
        var input = new WaiveIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        claim.WaiveIssue(input);

        var waived = claim.PopEvents().OfType<ValidationIssueWaivedEvent>().Should().ContainSingle().Subject;
        waived.IssueId.Should().Be(issue.Id);
        waived.Code.Should().Be(issue.Finding.Code);
        waived.ActorId.Should().Be(TestClaims.Actor);
    }

    [Fact]
    [Trait("Rule", HasLinkedPolicy.Code)]
    public void AcknowledgeIssue_RaisesOneAcknowledgedEvent()
    {
        var claim = TestClaims.WithPolicyWarning();
        claim.PopEvents();
        var issue = claim.Issues.Single(candidate => candidate.Finding.Code == HasLinkedPolicy.Code);
        var input = new AcknowledgeIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        claim.AcknowledgeIssue(input);

        var events = claim.PopEvents().OfType<ValidationIssueAcknowledgedEvent>().ToList();
        events.Should().ContainSingle().Which.Code.Should().Be(HasLinkedPolicy.Code);
    }

    [Fact]
    public void Open_TouchesTheClaimAndRaisesOneStateChangedEventWithTheActorAndBothStates()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new OpenClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        claim.Open(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
        var changed = claim.PopEvents().OfType<ClaimStateChangedEvent>().Should().ContainSingle().Subject;
        changed.ClaimId.Should().Be(claim.Id);
        changed.ActorId.Should().Be(TestClaims.Actor);
        changed.From.Should().Be("Draft");
        changed.To.Should().Be("Open");
    }

    [Fact]
    [Trait("Rule", "S:ST-Open-Withdrawn")]
    public void Withdraw_RaisesTheReasonOnTheStateChangedEvent()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var reason = new StateChangeReason("CUSTOMER_REQUEST", "The insured withdrew the claim");
        var input = new WithdrawClaimInput(
            TestClaims.Handler,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference())
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
        var input = new ReopenClaimInput(
            TestClaims.Supervisor,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference())
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
        var input = new CreateClaimInput(
            TestClaims.Organisation,
            ClaimNumber.From("CLM-2026-0000001"),
            TestClaims.Actor,
            TestClaims.Start,
            TestClaims.Reference());

        var outcome = Claim.Create(input);

        var claim = outcome.Value;
        var raised = claim.PopEvents().OfType<ValidationIssueRaisedEvent>().ToList();
        raised.Select(issue => issue.Code).Should().BeEquivalentTo(claim.Issues.Select(issue => issue.Finding.Code));
        raised.Should().OnlyContain(issue => issue.ClaimId == claim.Id && issue.ActorId == TestClaims.Actor);
    }

    [Fact]
    public void EvaluateIssues_AfterAFix_RaisesAClearedEventForEachClearedIssue()
    {
        var claim = TestClaims.Empty();
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var update = new UpdateLossDetailsInput(loss, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());
        TestClaims.ChangeLoss(claim, update);
        claim.PopEvents();
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());

        claim.EvaluateIssues(input);

        var cleared = claim.PopEvents().OfType<ValidationIssueClearedEvent>().ToList();
        string[] clearedCodes = [IsLossDatePresent.Code, IsDescriptionLongEnough.Code, IsCauseCodeActive.Code];
        cleared.Select(issue => issue.Code).Should().BeEquivalentTo(clearedCodes);
        cleared.Should().OnlyContain(issue => issue.ActorId == TestClaims.Actor);
    }

    [Fact]
    public void EvaluateIssues_WhenAnIssueIsRaised_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var loss = TestClaims.Loss(TestClaims.Later.AddDays(5), TestClaims.Description);
        var update = new UpdateLossDetailsInput(loss, TestClaims.Actor, TestClaims.Start, TestClaims.Reference());
        TestClaims.ChangeLoss(claim, update);
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());

        claim.EvaluateIssues(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void EvaluateIssues_WhenNothingChanges_LeavesTheClaimUntouched()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var updatedAt = claim.UpdatedAt;
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());

        claim.EvaluateIssues(input);

        claim.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void SetSeverity_WhenNoIssueChanges_RaisesNoIssueEvents()
    {
        var claim = TestClaims.Empty();
        var currentBefore = claim.Issues.Count(issue => issue.IsCurrent);
        claim.PopEvents();
        var input = new SetSeverityInput(
            ClaimSeverity.Critical,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        claim.SetSeverity(input);

        claim.PopEvents().OfType<ValidationIssueRaisedEvent>().Should().BeEmpty();
        claim.Issues.Count(issue => issue.IsCurrent).Should().Be(currentBefore);
    }

    [Fact]
    public void UpdateRiskObject_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var details = new RiskObjectDetails(AssetType.Equipment, "Pump");
        var input = new UpdateRiskObjectInput(
            claim.RiskObjects[0].Id,
            details,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        claim.UpdateRiskObject(input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void UpdateLossDetails_ForAClosedClaim_ReturnsTheFindingAndKeepsTheLoss()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var before = claim.LossEvent.Details;
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var input = new UpdateLossDetailsInput(loss, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = claim.UpdateLossDetails(input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
        claim.LossEvent.Details.Should().Be(before);
    }

    [Fact]
    public void UpdateLossDetails_AfterDraftWithAnEmptyLoss_IsRefusedAndTouchesNothing()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var updatedAt = claim.UpdatedAt;
        var input = new UpdateLossDetailsInput(
            LossDetails.Empty,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = claim.UpdateLossDetails(input);

        outcome.IsSuccess.Should().BeFalse();
        claim.UpdatedAt.Should().Be(updatedAt);
    }
}
