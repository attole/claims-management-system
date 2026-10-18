using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Claims.StateChanges.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class WithdrawClaimTransitionTests
{
    private static readonly StateChangeReason _reason = new("CUSTOMER_REQUEST", "The insured withdrew the claim");

    [Fact]
    [Trait("Rule", "S:ST-Open-Withdrawn")]
    public void Run_FromOpenWithAReason_WithdrawsTheClaimAndRecordsTheReason()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput();

        var outcome = WithdrawClaimTransition.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimWithdrawnState>();
        claim.StateChanges.Last().Reason.Should().Be(_reason);
    }

    [Fact]
    [Trait("Rule", "S:ST-UnderInvestigation-Withdrawn")]
    public void Run_FromUnderInvestigationWithAReason_WithdrawsTheClaim()
    {
        var claim = TestClaims.InState<ClaimUnderInvestigationState>();
        var input = CreateInput();

        var outcome = WithdrawClaimTransition.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", HasStateChangeReason.Code)]
    public void Run_WithoutAReason_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput() with { Reason = StateChangeReason.Empty };

        var outcome = WithdrawClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasStateChangeReason.Code);
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Run_FromADraft_ReturnsNotPermitted()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = CreateInput();

        var outcome = WithdrawClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(WithdrawClaimTransition.NotPermittedCode);
    }

    [Fact]
    [Trait("Rule", HasNoPendingReserveHistoryForWithdrawal.Code)]
    public void Run_WithAReserveHistoryAwaitingApproval_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput() with { HasPendingReserveHistory = true };

        var outcome = WithdrawClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(HasNoPendingReserveHistoryForWithdrawal.Code);
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", HasNoUnpostedReserveHistory.Code)]
    public void Run_WithAnApprovedChangeNotYetPosted_ReturnsTheFinding()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput() with { HasUnpostedReserveHistory = true };

        var outcome = WithdrawClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasNoUnpostedReserveHistory.Code);
    }

    [Fact]
    [Trait("Rule", HasWithdrawalNoteForOpenReserves.Code)]
    public void Run_WithAnOpenReserveBalanceAndNoNote_ReturnsTheFinding()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput() with { HasOpenReserveBalance = true };

        var outcome = WithdrawClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasWithdrawalNoteForOpenReserves.Code);
    }

    [Fact]
    [Trait("Rule", HasNoOpenReserveBalanceOnWithdrawal.Code)]
    public void Run_WithAnOpenReserveBalanceAndANote_WithdrawsWithTheWarning()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = CreateInput() with { HasOpenReserveBalance = true, Note = "Reserves released by finance" };

        var outcome = WithdrawClaimTransition.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.WarningFindings.Should().ContainSingle()
            .Which.Code.Should().Be(HasNoOpenReserveBalanceOnWithdrawal.Code);
    }

    private static FreezeClaimInput CreateInput()
    {
        return new FreezeClaimInput(false, false, false, TestClaims.Period(), TestClaims.Codes()) { Reason = _reason };
    }
}
