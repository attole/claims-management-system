using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Flows;

public sealed class RetractReserveTransitionTests
{
    [Fact]
    [Trait("Rule", IsSubmitter.Code)]
    public void Run_BySubmitter_CancelsThePendingRow()
    {
        var change = TestReserves.Submit(50_000m);
        var input = CreateInput(TestReserves.Submitter);

        var outcome = RetractReserveTransition.Run(change, input);

        outcome.Value.To.Should().BeOfType<ReserveCancelledState>();
        change.Balances.IsSet.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", IsSubmitter.Code)]
    public void Run_ByAnotherUser_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(50_000m);
        var input = CreateInput(TestReserves.Approver);

        var outcome = RetractReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsSubmitter.Code);
        change.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Run_FromAnAutoApprovedRow_ReturnsNotPermitted()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput(TestReserves.Submitter);

        var outcome = RetractReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(RetractReserveTransition.NotPermittedCode);
    }

    private static ReserveDecisionInput CreateInput(Guid actorId)
    {
        return new ReserveDecisionInput(actorId, TestClaims.Later);
    }
}
