using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Flows;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Flows;

public sealed class CloseClaimTransitionTests
{
    private const string ClosingNote = "Settled outside the system";

    [Fact]
    [Trait("Rule", "S:ST-Open-Closed")]
    public void Execute_FromOpenWhenEveryConditionHolds_Closes()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput(TestClaims.Reserves());

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Value.To.Should().BeOfType<ClaimClosedState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-UnderInvestigation-Closed")]
    public void Execute_FromUnderInvestigationWhenEveryConditionHolds_Closes()
    {
        var claim = TestClaims.InState<ClaimUnderInvestigationState>();
        var input = CreateInput(TestClaims.Reserves());

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Value.To.Should().BeOfType<ClaimClosedState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-PendingPayment-Closed")]
    public void Execute_FromPendingPaymentWhenEveryConditionHolds_Closes()
    {
        var claim = TestClaims.InState<ClaimPendingPaymentState>();
        var input = CreateInput(TestClaims.Reserves());

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Value.To.Should().BeOfType<ClaimClosedState>();
    }

    [Fact]
    [Trait("Rule", HasNoPendingReserveChange.Code)]
    public void Execute_WithAReserveChangeAwaitingApproval_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput(TestClaims.Reserves() with { HasPendingReserveChange = true });

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasNoPendingReserveChange.Code);
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", HasNoUnresolvedCriticalIssues.Code)]
    public void Execute_WithAnUnresolvedCriticalIssue_ReturnsTheFinding()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start.AddDays(2));
        TestClaims.MoveTo<ClaimOpenState>(claim);
        var input = CreateInput(TestClaims.Reserves());

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasNoUnresolvedCriticalIssues.Code);
    }

    [Fact]
    [Trait("Rule", HasClaimantForClosure.Code)]
    public void Execute_WithoutAClaimant_ReturnsTheClaimantFinding()
    {
        var claim = TestClaims.InStateWithoutClaimant<ClaimOpenState>();
        var input = CreateInput(TestClaims.Reserves());

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Contain(HasClaimantForClosure.Code);
    }

    [Fact]
    [Trait("Rule", HasClosingNoteForOpenReserves.Code)]
    public void Execute_WithAnOpenReserveBalanceAndNoNote_ReturnsTheFinding()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput(TestClaims.Reserves() with { HasOpenReserveBalance = true });

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasClosingNoteForOpenReserves.Code);
    }

    [Fact]
    [Trait("Rule", HasClosingNoteForOpenReserves.Code)]
    public void Execute_WithAnOpenReserveBalanceAndANote_ClosesAndStoresTheNote()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var reserves = TestClaims.Reserves() with { HasOpenReserveBalance = true };
        var input = CreateInput(reserves) with { Note = ClosingNote };

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.StateChanges.Last().Note.Should().Be(ClosingNote);
    }

    [Fact]
    [Trait("Rule", HasNoOpenReserveBalance.Code)]
    public void Execute_WithAnOpenReserveBalanceAndANote_ReturnsTheAdvisoryFinding()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var reserves = TestClaims.Reserves() with { HasOpenReserveBalance = true };
        var input = CreateInput(reserves) with { Note = ClosingNote };

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Advisory.Should().ContainSingle().Which.Code.Should().Be(HasNoOpenReserveBalance.Code);
    }

    [Fact]
    [Trait("Rule", HasNoOpenReserveBalance.Code)]
    public void Execute_WithoutAnOpenReserveBalance_ReturnsNoAdvisoryFinding()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput(TestClaims.Reserves());

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Advisory.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-03")]
    public void Execute_WithSeveralFailedConditions_ReturnsEveryFinding()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var reserves = TestClaims.Reserves() with { HasPendingReserveChange = true, HasOpenReserveBalance = true };
        var input = CreateInput(reserves);

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Blocking.Select(finding => finding.Code)
            .Should().BeEquivalentTo(HasNoPendingReserveChange.Code, HasClosingNoteForOpenReserves.Code);
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Execute_FromADraft_ReturnsNotPermitted()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = CreateInput(TestClaims.Reserves());

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Execute_FromClosed_ReturnsNotPermitted()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = CreateInput(TestClaims.Reserves());

        var outcome = CloseClaimTransition.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }

    private static CloseClaimInput CreateInput(ClaimReserveSnapshot reserves)
    {
        return new CloseClaimInput(
            TestClaims.Handler,
            TestClaims.Actor,
            TestClaims.Later,
            reserves,
            TestClaims.Reference());
    }
}
