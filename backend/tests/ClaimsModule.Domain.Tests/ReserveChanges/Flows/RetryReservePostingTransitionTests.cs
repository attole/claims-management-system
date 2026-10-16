using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Flows;

public sealed class RetryReservePostingTransitionTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_FromFailed_ReturnsToPendingWithTheNewJob()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var failed = new ReservePostingInput { JobId = "job-1", FailureReason = "timeout" };
        MarkReservePostingFailedTransition.Run(change, failed);
        var input = new ReservePostingInput { JobId = "job-2" };

        var outcome = RetryReservePostingTransition.Run(change, input);

        outcome.Value.To.Should().BeOfType<ReservePostingPendingState>();
        change.PostingDetails.JobId.Should().Be("job-2");
        change.PostingDetails.FailureReason.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_FromPending_ReturnsNotPermitted()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = new ReservePostingInput { JobId = "job-2" };

        var outcome = RetryReservePostingTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(RetryReservePostingTransition.NotPermittedCode);
    }

    [Fact]
    [Trait("Rule", HasPostingJob.Code)]
    public void Run_WithoutAJob_ReturnsTheFinding()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var failed = new ReservePostingInput { JobId = "job-1", FailureReason = "timeout" };
        MarkReservePostingFailedTransition.Run(change, failed);
        var input = new ReservePostingInput();

        var outcome = RetryReservePostingTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasPostingJob.Code);
    }
}
