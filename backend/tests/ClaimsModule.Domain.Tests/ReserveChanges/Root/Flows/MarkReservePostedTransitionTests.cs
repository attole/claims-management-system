using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.ReserveChanges.Root.Flows;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Flows;

public sealed class MarkReservePostedTransitionTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public void Execute_FromPendingForAnApprovedChange_PostsAndStoresTheJobId()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput("job-1");

        var outcome = MarkReservePostedTransition.Execute(reserve, input);

        outcome.Value.To.Should().BeOfType<ReservePostingPostedState>();
        reserve.PostingDetails.JobId.Should().Be("job-1");
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Execute_FromFailed_PostsAgainOnRetry()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var failed = new MarkReservePostingFailedInput(TestClaims.Actor, TestClaims.Later)
        {
            FailureReason = "timeout",
        };
        MarkReservePostingFailedTransition.Execute(reserve, failed);
        var input = CreateInput("job-2");

        var outcome = MarkReservePostedTransition.Execute(reserve, input);

        outcome.Value.To.Should().BeOfType<ReservePostingPostedState>();
        reserve.PostingDetails.JobId.Should().Be("job-2");
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Idempotent")]
    [Trait("Rule", "S:BR-R-06")]
    public void Execute_Twice_ReturnsNotPermittedTheSecondTimeAndChangesNothing()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput("job-1");
        var second = CreateInput("job-2");
        MarkReservePostedTransition.Execute(reserve, input);
        reserve.PopEvents();

        var outcome = MarkReservePostedTransition.Execute(reserve, second);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
        reserve.PostingDetails.JobId.Should().Be("job-1");
        reserve.PopEvents().Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReserveChangeApproved.Code)]
    public void Execute_ForAChangeThatIsStillPending_ReturnsTheFindingAndKeepsThePosting()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput("job-1");

        var outcome = MarkReservePostedTransition.Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeApproved.Code);
        reserve.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    private static MarkReservePostedInput CreateInput(string jobId)
    {
        return new MarkReservePostedInput(TestClaims.Actor, TestClaims.Later) { JobId = jobId };
    }
}
