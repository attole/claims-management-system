using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.ReserveChanges.Root.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Transitions;

public sealed class AutoApproveReserveTransitionTests
{
    [Fact]
    [Trait("Rule", "S:BR-R-02")]
    [Trait("Rule", "S:RES-EventSourced")]
    public void Execute_WithinTheAutoLimit_AutoApprovesAndFillsTheBalances()
    {
        var reserve = TestReserves.Submit(5_000m);
        var input = CreateInput(TestReserves.Snapshot());

        var outcome = new AutoApproveReserveTransition().Execute(reserve, input);

        outcome.Value.Should().BeOfType<ReserveAutoApprovedState>();
        reserve.Balances.PreviousBalance.Should().Be(0m);
        reserve.Balances.NewBalance.Should().Be(5_000m);
        reserve.Review.ReviewedBy.Should().Be(TestReserves.Submitter);
    }

    [Fact]
    [Trait("Rule", IsWithinAutoApprovalLimit.Code)]
    public void Execute_AboveTheAutoLimit_ReturnsTheFindingAndKeepsTheRowPending()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput(TestReserves.Snapshot());

        var outcome = new AutoApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsWithinAutoApprovalLimit.Code);
        reserve.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Execute_WhenASmallAmountPassesTheClaimLimit_ReturnsTheFindingAndKeepsTheRowPending()
    {
        var snapshot = TestReserves.Snapshot() with { ClaimApprovedTotal = 9_995_000m };
        var reserve = TestReserves.Submit(8_000m, snapshot);
        var input = CreateInput(snapshot);

        var outcome = new AutoApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimLimitCleared.Code);
        reserve.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Execute_WhenTheOverrideIsSet_StillKeepsTheRowPending()
    {
        var snapshot = TestReserves.Snapshot() with { ClaimApprovedTotal = 9_995_000m, HasLimitOverride = true };
        var reserve = TestReserves.Submit(8_000m, snapshot);
        var input = CreateInput(snapshot);

        var outcome = new AutoApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimLimitCleared.Code);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Execute_FromAnApprovedRow_ReturnsNotPermitted()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput(TestReserves.Snapshot());

        var outcome = new AutoApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }

    private static AutoApproveReserveInput CreateInput(ReserveSnapshot snapshot)
    {
        return new AutoApproveReserveInput(TestClaims.Handler, TestReserves.Submitter, TestClaims.Later, snapshot);
    }
}
