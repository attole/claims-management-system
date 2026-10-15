using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.ReserveChanges.Root.Events;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.ReserveChanges.Root.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Transitions;

public sealed class ApproveReserveTransitionTests
{
    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    [Trait("Rule", "S:RES-EventSourced")]
    public void Execute_FromPendingBySupervisor_ApprovesAndFillsTheBalances()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput(TestReserves.Snapshot());

        var outcome = new ApproveReserveTransition().Execute(reserve, input);

        outcome.Value.Should().BeOfType<ReserveApprovedState>();
        reserve.Balances.PreviousBalance.Should().Be(0m);
        reserve.Balances.NewBalance.Should().Be(50_000m);
        reserve.Review.ReviewedBy.Should().Be(TestReserves.Approver);
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Execute_FromPending_RaisesOneApprovalStateChangedEvent()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput(TestReserves.Snapshot());

        new ApproveReserveTransition().Execute(reserve, input);

        var changed = reserve.PopEvents().OfType<ReserveApprovalStateChangedEvent>().Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReserveApprovalStateKind.PendingApproval));
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.Approved));
        changed.ActorId.Should().Be(TestReserves.Approver);
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    public void Execute_ByAHandler_ReturnsTheFindingAndKeepsTheApproval()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = new ApproveReserveInput(
            TestClaims.Handler,
            TestReserves.Approver,
            TestClaims.Later,
            TestReserves.Snapshot());

        var outcome = new ApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasApprovalAuthority.Code);
        reserve.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    public void Execute_ByASupervisorAboveTheirLimit_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(250_000m);
        var input = CreateInput(TestReserves.Snapshot());

        var outcome = new ApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasApprovalAuthority.Code);
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    public void Execute_ByAManagerAboveTenMillion_Approves()
    {
        var snapshot = TestReserves.Snapshot() with { HasLimitOverride = true };
        var reserve = TestReserves.Submit(12_000_000m, snapshot);
        var input = new ApproveReserveInput(TestClaims.Manager, TestReserves.Approver, TestClaims.Later, snapshot);

        var outcome = new ApproveReserveTransition().Execute(reserve, input);

        outcome.Value.Should().BeOfType<ReserveApprovedState>();
    }

    [Fact]
    [Trait("Rule", IsNotSelfApproval.Code)]
    public void Execute_BySubmitter_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = new ApproveReserveInput(
            TestClaims.Manager,
            TestReserves.Submitter,
            TestClaims.Later,
            TestReserves.Snapshot());

        var outcome = new ApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsNotSelfApproval.Code);
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Execute_WhenTheClaimLimitIsPassedWithoutAnOverride_ReturnsTheFinding()
    {
        var snapshot = TestReserves.Snapshot() with { ClaimApprovedTotal = 9_995_000m };
        var reserve = TestReserves.Submit(8_000m, snapshot);
        var input = CreateInput(snapshot);

        var outcome = new ApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimLimitCleared.Code);
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Execute_WhenTheClaimLimitIsPassedWithAnOverride_Approves()
    {
        var snapshot = TestReserves.Snapshot() with { ClaimApprovedTotal = 9_995_000m, HasLimitOverride = true };
        var reserve = TestReserves.Submit(8_000m, snapshot);
        var input = CreateInput(snapshot);

        var outcome = new ApproveReserveTransition().Execute(reserve, input);

        outcome.Value.Should().BeOfType<ReserveApprovedState>();
    }

    [Fact]
    [Trait("Rule", IsApprovedBalanceNonNegative.Code)]
    public void Execute_WhenTheBalanceHasFallenSinceSubmission_ReturnsTheFinding()
    {
        var submitSnapshot = TestReserves.Snapshot() with { ReserveApprovedBalance = 100_000m };
        var reserve = TestReserves.Submit(-50_000m, submitSnapshot);
        var input = CreateInput(TestReserves.Snapshot() with { ReserveApprovedBalance = 10_000m });

        var outcome = new ApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsApprovedBalanceNonNegative.Code);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Execute_FromAnAutoApprovedRow_ReturnsNotPermitted()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput(TestReserves.Snapshot());

        var outcome = new ApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Execute_Twice_ReturnsNotPermittedTheSecondTimeAndChangesNothing()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput(TestReserves.Snapshot());
        new ApproveReserveTransition().Execute(reserve, input);
        reserve.PopEvents();

        var outcome = new ApproveReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
        reserve.PopEvents().Should().BeEmpty();
    }

    private static ApproveReserveInput CreateInput(ReserveSnapshot snapshot)
    {
        return new ApproveReserveInput(TestClaims.Supervisor, TestReserves.Approver, TestClaims.Later, snapshot);
    }
}
