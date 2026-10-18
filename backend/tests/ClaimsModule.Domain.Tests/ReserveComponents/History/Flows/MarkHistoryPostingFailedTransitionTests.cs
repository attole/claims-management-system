using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class MarkHistoryPostingFailedTransitionTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_FromPendingForAnApprovedChange_FailsAndStoresTheReason()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();

        var outcome = MarkHistoryPostingFailedTransition.Run(history, input);

        outcome.Value.To.Should().BeOfType<ReservePostingFailedState>();
        history.PostingDetails.FailureReason.Should().Be("The ledger timed out");
    }

    [Fact]
    [Trait("Rule", IsReserveHistoryApproved.Code)]
    public void Run_ForAChangeThatIsStillPending_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(50_000m);
        var input = CreateInput();

        var outcome = MarkHistoryPostingFailedTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsReserveHistoryApproved.Code);
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_FromFailed_ReturnsNotPermitted()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();
        MarkHistoryPostingFailedTransition.Run(history, input);

        var outcome = MarkHistoryPostingFailedTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(MarkHistoryPostingFailedTransition.NotPermittedCode);
    }

    [Fact]
    [Trait("Rule", HasPostingJob.Code)]
    public void Run_WithoutAJob_ReturnsTheFinding()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = new RowPostingInput();

        var outcome = MarkHistoryPostingFailedTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasPostingJob.Code);
    }

    private static RowPostingInput CreateInput()
    {
        return new RowPostingInput()
        {
            JobId = "job-1",
            FailureReason = "The ledger timed out",
        };
    }
}
