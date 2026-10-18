using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class RetractHistoryTransitionTests
{
    [Fact]
    [Trait("Rule", IsSubmitter.Code)]
    public void Run_BySubmitter_CancelsThePendingRow()
    {
        var history = TestReserves.Submit(50_000m);
        var input = CreateInput(TestReserves.Submitter);

        var outcome = RetractHistoryTransition.Run(history, input);

        outcome.Value.To.Should().BeOfType<ReserveCancelledState>();
        history.Balances.IsSet.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", IsSubmitter.Code)]
    public void Run_ByAnotherUser_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(50_000m);
        var input = CreateInput(TestReserves.Approver);

        var outcome = RetractHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsSubmitter.Code);
        history.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Run_FromAnAutoApprovedRow_ReturnsNotPermitted()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput(TestReserves.Submitter);

        var outcome = RetractHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(RetractHistoryTransition.NotPermittedCode);
    }

    private static RowDecisionInput CreateInput(Guid actorId)
    {
        return new RowDecisionInput(actorId, TestClaims.Later);
    }
}
