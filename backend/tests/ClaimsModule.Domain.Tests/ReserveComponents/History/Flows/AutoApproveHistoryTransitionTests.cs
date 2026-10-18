using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class AutoApproveHistoryTransitionTests
{
    [Fact]
    [Trait("Rule", "S:BR-R-02")]
    [Trait("Rule", "S:RES-EventSourced")]
    [Trait("Rule", "A:BR-R-02")]
    public void Run_WithinTheAutoLimit_AutoApprovesAndFillsTheBalances()
    {
        var history = TestReserves.Submit(5_000m);
        var input = CreateInput();

        var outcome = AutoApproveHistoryTransition.Run(history, input);

        outcome.Value.To.Should().BeOfType<ReserveAutoApprovedState>();
        history.Balances.PreviousBalance.Should().Be(0m);
        history.Balances.NewBalance.Should().Be(5_000m);
        history.Review.ReviewedBy.Should().Be(Guid.Empty);
    }

    [Fact]
    [Trait("Rule", IsWithinAutoApprovalLimit.Code)]
    public void Run_AboveTheAutoLimit_ReturnsTheFindingAndKeepsTheRowPending()
    {
        var history = TestReserves.Submit(50_000m);
        var input = CreateInput();

        var outcome = AutoApproveHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsWithinAutoApprovalLimit.Code);
        history.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Run_WhenASmallAmountPassesTheClaimLimit_ReturnsTheFindingAndKeepsTheRowPending()
    {
        var history = TestReserves.Submit(8_000m);
        var input = CreateInput() with { ClaimApprovedTotal = 9_995_000m };

        var outcome = AutoApproveHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimLimitCleared.Code);
        history.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Run_FromAnApprovedRow_ReturnsNotPermitted()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();

        var outcome = AutoApproveHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(AutoApproveHistoryTransition.NotPermittedCode);
    }

    private static AutoApproveRowInput CreateInput()
    {
        return TestReserves.AutoApproveInput();
    }
}
