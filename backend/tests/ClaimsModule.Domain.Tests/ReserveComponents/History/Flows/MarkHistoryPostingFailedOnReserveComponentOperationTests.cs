using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class MarkHistoryPostingFailedOnReserveComponentOperationTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_MarksTheApprovedRowFailedWithTheReason()
    {
        var component = TestReserves.Component();
        var history = component.SubmitHistory(TestReserves.SubmitInput(5_000m)).Value;
        var input = new HistoryPostingInput(history.Id) { JobId = "job-1", FailureReason = "Ledger unavailable" };

        var outcome = MarkHistoryPostingFailedOnReserveComponentOperation.Run(component, input);

        outcome.IsSuccess.Should().BeTrue();
        history.PostingState.Should().BeOfType<ReservePostingFailedState>();
        history.PostingDetails.FailureReason.Should().Be("Ledger unavailable");
    }

    [Fact]
    public void Run_ForAnUnknownRow_Throws()
    {
        var component = TestReserves.Component();
        var input = new HistoryPostingInput(Guid.NewGuid()) { JobId = "job-1" };

        Action act = () => MarkHistoryPostingFailedOnReserveComponentOperation.Run(component, input);

        act.Should().Throw<InvalidOperationException>();
    }
}
