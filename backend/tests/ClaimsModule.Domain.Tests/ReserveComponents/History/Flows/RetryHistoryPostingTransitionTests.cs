using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class RetryHistoryPostingTransitionTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_FromFailed_ReturnsToPendingWithTheNewJob()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var failed = new RowPostingInput { JobId = "job-1", FailureReason = "timeout" };
        MarkHistoryPostingFailedTransition.Run(history, failed);
        var input = new RowPostingInput { JobId = "job-2" };

        var outcome = RetryHistoryPostingTransition.Run(history, input);

        outcome.Value.To.Should().BeOfType<ReservePostingPendingState>();
        history.PostingDetails.JobId.Should().Be("job-2");
        history.PostingDetails.FailureReason.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_FromPending_ReturnsNotPermitted()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = new RowPostingInput { JobId = "job-2" };

        var outcome = RetryHistoryPostingTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(RetryHistoryPostingTransition.NotPermittedCode);
    }

    [Fact]
    [Trait("Rule", HasPostingJob.Code)]
    public void Run_WithoutAJob_ReturnsTheFinding()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var failed = new RowPostingInput { JobId = "job-1", FailureReason = "timeout" };
        MarkHistoryPostingFailedTransition.Run(history, failed);
        var input = new RowPostingInput();

        var outcome = RetryHistoryPostingTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasPostingJob.Code);
    }
}
