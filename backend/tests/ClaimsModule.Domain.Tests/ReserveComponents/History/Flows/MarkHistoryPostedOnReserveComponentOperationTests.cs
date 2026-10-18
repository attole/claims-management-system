using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class MarkHistoryPostedOnReserveComponentOperationTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public void Run_MarksTheApprovedRowPostedWithTheJob()
    {
        var component = TestReserves.Component();
        var history = component.SubmitHistory(TestReserves.SubmitInput(5_000m)).Value;
        var input = new HistoryPostingInput(history.Id) { JobId = "job-1" };

        var outcome = MarkHistoryPostedOnReserveComponentOperation.Run(component, input);

        outcome.IsSuccess.Should().BeTrue();
        history.PostingState.Should().BeOfType<ReservePostingPostedState>();
        history.PostingDetails.JobId.Should().Be("job-1");
    }

    [Fact]
    public void Run_ForAnUnknownRow_Throws()
    {
        var component = TestReserves.Component();
        var input = new HistoryPostingInput(Guid.NewGuid()) { JobId = "job-1" };

        Action act = () => MarkHistoryPostedOnReserveComponentOperation.Run(component, input);

        act.Should().Throw<InvalidOperationException>();
    }
}
