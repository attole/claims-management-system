using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Flows;

public sealed class MarkReservePostingFailedTransitionTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_FromPendingForAnApprovedChange_FailsAndStoresTheReason()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();

        var outcome = MarkReservePostingFailedTransition.Run(change, input);

        outcome.Value.To.Should().BeOfType<ReservePostingFailedState>();
        change.PostingDetails.FailureReason.Should().Be("The ledger timed out");
    }

    [Fact]
    [Trait("Rule", IsReserveChangeApproved.Code)]
    public void Run_ForAChangeThatIsStillPending_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(50_000m);
        var input = CreateInput();

        var outcome = MarkReservePostingFailedTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeApproved.Code);
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Run_FromFailed_ReturnsNotPermitted()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();
        MarkReservePostingFailedTransition.Run(change, input);

        var outcome = MarkReservePostingFailedTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(MarkReservePostingFailedTransition.NotPermittedCode);
    }

    [Fact]
    [Trait("Rule", HasPostingJob.Code)]
    public void Run_WithoutAJob_ReturnsTheFinding()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = new ReservePostingInput();

        var outcome = MarkReservePostingFailedTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasPostingJob.Code);
    }

    private static ReservePostingInput CreateInput()
    {
        return new ReservePostingInput()
        {
            JobId = "job-1",
            FailureReason = "The ledger timed out",
        };
    }
}
