using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class MarkHistoryPostedTransitionTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public void Run_FromPendingForAnApprovedChange_PostsAndStoresTheJobId()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput("job-1");

        var outcome = MarkHistoryPostedTransition.Run(history, input);

        outcome.Value.To.Should().BeOfType<ReservePostingPostedState>();
        history.PostingDetails.JobId.Should().Be("job-1");
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Idempotent")]
    [Trait("Rule", "S:BR-R-06")]
    public void Run_Twice_ReturnsNotPermittedTheSecondTimeAndChangesNothing()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput("job-1");
        var second = CreateInput("job-2");
        MarkHistoryPostedTransition.Run(history, input);

        var outcome = MarkHistoryPostedTransition.Run(history, second);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(MarkHistoryPostedTransition.NotPermittedCode);
        history.PostingDetails.JobId.Should().Be("job-1");
    }

    [Fact]
    [Trait("Rule", IsReserveHistoryApproved.Code)]
    public void Run_ForAChangeThatIsStillPending_ReturnsTheFindingAndKeepsThePosting()
    {
        var history = TestReserves.Submit(50_000m);
        var input = CreateInput("job-1");

        var outcome = MarkHistoryPostedTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsReserveHistoryApproved.Code);
        history.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_FromFailed_ReturnsNotPermitted()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var failed = new RowPostingInput { JobId = "job-1", FailureReason = "timeout" };
        MarkHistoryPostingFailedTransition.Run(history, failed);
        var input = CreateInput("job-2");

        var outcome = MarkHistoryPostedTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(MarkHistoryPostedTransition.NotPermittedCode);
    }

    [Fact]
    [Trait("Rule", HasPostingJob.Code)]
    public void Run_WithoutAJob_ReturnsTheFinding()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = new RowPostingInput();

        var outcome = MarkHistoryPostedTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasPostingJob.Code);
    }

    private static RowPostingInput CreateInput(string jobId)
    {
        return new RowPostingInput() { JobId = jobId };
    }
}
