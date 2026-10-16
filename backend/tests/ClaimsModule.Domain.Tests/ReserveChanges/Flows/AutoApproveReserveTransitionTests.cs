using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Flows;

public sealed class AutoApproveReserveTransitionTests
{
    [Fact]
    [Trait("Rule", "S:BR-R-02")]
    [Trait("Rule", "S:RES-EventSourced")]
    [Trait("Rule", "A:BR-R-02")]
    public void Run_WithinTheAutoLimit_AutoApprovesAndFillsTheBalances()
    {
        var change = TestReserves.Submit(5_000m);
        var input = CreateInput();

        var outcome = AutoApproveReserveTransition.Run(change, input);

        outcome.Value.To.Should().BeOfType<ReserveAutoApprovedState>();
        change.Balances.PreviousBalance.Should().Be(0m);
        change.Balances.NewBalance.Should().Be(5_000m);
        change.Review.ReviewedBy.Should().Be(Guid.Empty);
    }

    [Fact]
    [Trait("Rule", IsWithinAutoApprovalLimit.Code)]
    public void Run_AboveTheAutoLimit_ReturnsTheFindingAndKeepsTheRowPending()
    {
        var change = TestReserves.Submit(50_000m);
        var input = CreateInput();

        var outcome = AutoApproveReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsWithinAutoApprovalLimit.Code);
        change.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Run_WhenASmallAmountPassesTheClaimLimit_ReturnsTheFindingAndKeepsTheRowPending()
    {
        var change = TestReserves.Submit(8_000m);
        var input = CreateInput() with { ClaimApprovedTotal = 9_995_000m };

        var outcome = AutoApproveReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimLimitCleared.Code);
        change.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Run_FromAnApprovedRow_ReturnsNotPermitted()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();

        var outcome = AutoApproveReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(AutoApproveReserveTransition.NotPermittedCode);
    }

    private static AutoApproveReserveInput CreateInput()
    {
        return TestReserves.AutoApproveInput();
    }
}
