using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class RetryHistoryPostingOnReserveComponentOperationTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_ReturnsAFailedRowToPending()
    {
        var component = TestReserves.Component();
        var history = component.SubmitHistory(TestReserves.SubmitInput(5_000m)).Value;
        var failed = new HistoryPostingInput(history.Id) { JobId = "job-1", FailureReason = "timeout" };
        component.MarkHistoryPostingFailed(failed);
        var input = new HistoryPostingInput(history.Id) { JobId = "job-2" };

        var outcome = RetryHistoryPostingOnReserveComponentOperation.Run(component, input);

        outcome.IsSuccess.Should().BeTrue();
        history.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    public void Run_ForAnUnknownRow_Throws()
    {
        var component = TestReserves.Component();
        var input = new HistoryPostingInput(Guid.NewGuid()) { JobId = "job-2" };

        Action act = () => RetryHistoryPostingOnReserveComponentOperation.Run(component, input);

        act.Should().Throw<InvalidOperationException>();
    }
}
