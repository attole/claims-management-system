using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.ReserveChanges.Root.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Transitions;

public sealed class RetractReserveTransitionTests
{
    [Fact]
    [Trait("Rule", IsSubmitter.Code)]
    public void Execute_BySubmitter_CancelsThePendingRow()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Handler, TestReserves.Submitter);

        var outcome = new RetractReserveTransition().Execute(reserve, input);

        outcome.Value.Should().BeOfType<ReserveCancelledState>();
        reserve.Balances.IsSet.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", IsSubmitter.Code)]
    public void Execute_ByAnotherUser_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Manager, TestReserves.Approver);

        var outcome = new RetractReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsSubmitter.Code);
        reserve.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Execute_FromAnAutoApprovedRow_ReturnsNotPermitted()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput(TestClaims.Handler, TestReserves.Submitter);

        var outcome = new RetractReserveTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }

    private static RetractReserveInput CreateInput(UserRole role, Guid actorId)
    {
        return new RetractReserveInput(role, actorId, TestClaims.Later, TestReserves.Snapshot());
    }
}
