using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Flows;

public sealed class MarkReservePostedTransitionTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public void Run_FromPendingForAnApprovedChange_PostsAndStoresTheJobId()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput("job-1");

        var outcome = MarkReservePostedTransition.Run(change, input);

        outcome.Value.To.Should().BeOfType<ReservePostingPostedState>();
        change.PostingDetails.JobId.Should().Be("job-1");
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Idempotent")]
    [Trait("Rule", "S:BR-R-06")]
    public void Run_Twice_ReturnsNotPermittedTheSecondTimeAndChangesNothing()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput("job-1");
        var second = CreateInput("job-2");
        MarkReservePostedTransition.Run(change, input);

        var outcome = MarkReservePostedTransition.Run(change, second);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(MarkReservePostedTransition.NotPermittedCode);
        change.PostingDetails.JobId.Should().Be("job-1");
    }

    [Fact]
    [Trait("Rule", IsReserveChangeApproved.Code)]
    public void Run_ForAChangeThatIsStillPending_ReturnsTheFindingAndKeepsThePosting()
    {
        var change = TestReserves.Submit(50_000m);
        var input = CreateInput("job-1");

        var outcome = MarkReservePostedTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeApproved.Code);
        change.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_FromFailed_ReturnsNotPermitted()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var failed = new ReservePostingInput { JobId = "job-1", FailureReason = "timeout" };
        MarkReservePostingFailedTransition.Run(change, failed);
        var input = CreateInput("job-2");

        var outcome = MarkReservePostedTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(MarkReservePostedTransition.NotPermittedCode);
    }

    [Fact]
    [Trait("Rule", HasPostingJob.Code)]
    public void Run_WithoutAJob_ReturnsTheFinding()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = new ReservePostingInput();

        var outcome = MarkReservePostedTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasPostingJob.Code);
    }

    private static ReservePostingInput CreateInput(string jobId)
    {
        return new ReservePostingInput() { JobId = jobId };
    }
}
