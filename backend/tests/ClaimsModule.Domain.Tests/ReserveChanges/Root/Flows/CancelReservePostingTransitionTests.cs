using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.ReserveChanges.Root.Flows;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Flows;

public sealed class CancelReservePostingTransitionTests
{
    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Execute_ForARejectedChange_CancelsThePosting()
    {
        var reserve = TestReserves.SubmitRejected();
        var input = CreateInput();

        var outcome = CancelReservePostingTransition.Execute(reserve, input);

        outcome.Value.To.Should().BeOfType<ReservePostingCancelledState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Execute_ForARetractedChange_CancelsThePosting()
    {
        var reserve = TestReserves.SubmitRetracted();
        var input = CreateInput();

        var outcome = CancelReservePostingTransition.Execute(reserve, input);

        outcome.Value.To.Should().BeOfType<ReservePostingCancelledState>();
    }

    [Fact]
    [Trait("Rule", IsReserveChangeRefused.Code)]
    public void Execute_ForAnApprovedChange_ReturnsTheFindingAndKeepsThePosting()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();

        var outcome = CancelReservePostingTransition.Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeRefused.Code);
        reserve.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Execute_FromCancelled_ReturnsNotPermitted()
    {
        var reserve = TestReserves.SubmitRejected();
        var input = CreateInput();
        CancelReservePostingTransition.Execute(reserve, input);

        var outcome = CancelReservePostingTransition.Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }

    private static CancelReservePostingInput CreateInput()
    {
        return new CancelReservePostingInput(TestClaims.Actor, TestClaims.Later);
    }
}
