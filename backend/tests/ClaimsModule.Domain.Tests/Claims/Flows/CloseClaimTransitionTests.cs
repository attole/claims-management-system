using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class CloseClaimTransitionTests
{
    private const string ClosingNote = "Settled outside the system";

    [Fact]
    [Trait("Rule", "S:ST-Open-Closed")]
    public void Run_FromOpenWhenEveryConditionHolds_Closes()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput();

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.Value.To.Should().BeOfType<ClaimClosedState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-UnderInvestigation-Closed")]
    public void Run_FromUnderInvestigationWhenEveryConditionHolds_Closes()
    {
        var claim = TestClaims.InState<ClaimUnderInvestigationState>();
        var input = CreateInput();

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.Value.To.Should().BeOfType<ClaimClosedState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-PendingPayment-Closed")]
    public void Run_FromPendingPaymentWhenEveryConditionHolds_Closes()
    {
        var claim = TestClaims.InState<ClaimPendingPaymentState>();
        var input = CreateInput();

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.Value.To.Should().BeOfType<ClaimClosedState>();
    }

    [Fact]
    [Trait("Rule", HasNoPendingReserveHistory.Code)]
    public void Run_WithAReserveHistoryAwaitingApproval_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput() with { HasPendingReserveHistory = true };

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasNoPendingReserveHistory.Code);
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", HasNoUnpostedReserveHistory.Code)]
    public void Run_WithAnApprovedChangeNotYetPosted_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput() with { HasUnpostedReserveHistory = true };

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasNoUnpostedReserveHistory.Code);
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", HasNoCriticalIssuesForClose.Code)]
    public void Run_WithAnUnresolvedCriticalIssue_ReturnsTheFinding()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start.AddDays(2));
        TestClaims.MoveTo<ClaimOpenState>(claim);
        var input = CreateInput();

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasNoCriticalIssuesForClose.Code);
    }

    [Fact]
    [Trait("Rule", HasClaimantForClosure.Code)]
    public void Run_WithoutAClaimant_ReturnsTheClaimantFinding()
    {
        var claim = TestClaims.InStateWithoutClaimant<ClaimOpenState>();
        var input = CreateInput();

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Contain(HasClaimantForClosure.Code);
    }

    [Fact]
    [Trait("Rule", HasClosingNoteForOpenReserves.Code)]
    public void Run_WithAnOpenReserveBalanceAndNoNote_ReturnsTheFinding()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput() with { HasOpenReserveBalance = true };

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasClosingNoteForOpenReserves.Code);
    }

    [Fact]
    [Trait("Rule", HasNoOpenReserveBalance.Code)]
    public void Run_WithAnOpenReserveBalanceAndANote_ReturnsTheWarning()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput() with { HasOpenReserveBalance = true, Note = ClosingNote };

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.WarningFindings.Should().ContainSingle().Which.Code.Should().Be(HasNoOpenReserveBalance.Code);
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-03")]
    public void Run_WithSeveralFailedConditions_ReturnsEveryFinding()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput() with { HasPendingReserveHistory = true, HasOpenReserveBalance = true };

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Select(finding => finding.Code)
            .Should().BeEquivalentTo(HasNoPendingReserveHistory.Code, HasClosingNoteForOpenReserves.Code);
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Run_FromADraft_ReturnsNotPermitted()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = CreateInput();

        var outcome = CloseClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(CloseClaimTransition.NotPermittedCode);
    }

    private static FreezeClaimInput CreateInput()
    {
        return new FreezeClaimInput(false, false, false, TestClaims.Period(), TestClaims.Codes());
    }
}
