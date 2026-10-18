using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class CancelHistoryPostingTransitionTests
{
    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Run_ForARejectedChange_CancelsThePosting()
    {
        var history = TestReserves.SubmitRejected();
        var input = CreateInput();

        var outcome = CancelHistoryPostingTransition.Run(history, input);

        outcome.Value.To.Should().BeOfType<ReservePostingCancelledState>();
    }

    [Fact]
    [Trait("Rule", IsReserveHistoryRefused.Code)]
    public void Run_ForAnApprovedChange_ReturnsTheFindingAndKeepsThePosting()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();

        var outcome = CancelHistoryPostingTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsReserveHistoryRefused.Code);
        history.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Run_FromCancelled_ReturnsNotPermitted()
    {
        var history = TestReserves.SubmitRejected();
        var input = CreateInput();
        CancelHistoryPostingTransition.Run(history, input);

        var outcome = CancelHistoryPostingTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(CancelHistoryPostingTransition.NotPermittedCode);
    }

    private static RowPostingInput CreateInput()
    {
        return new RowPostingInput();
    }
}
