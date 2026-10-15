using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.ReserveChanges.Root.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Transitions;

public sealed class MarkReservePostingFailedTransitionTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Execute_FromPendingForAnApprovedChange_FailsAndStoresTheReason()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();

        var outcome = new MarkReservePostingFailedTransition().Execute(reserve, input);

        outcome.Value.Should().BeOfType<ReservePostingFailedState>();
        reserve.PostingDetails.FailureReason.Should().Be("The ledger timed out");
    }

    [Fact]
    [Trait("Rule", IsReserveChangeApproved.Code)]
    public void Execute_ForAChangeThatIsStillPending_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput();

        var outcome = new MarkReservePostingFailedTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeApproved.Code);
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void Execute_FromFailed_ReturnsNotPermitted()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();
        new MarkReservePostingFailedTransition().Execute(reserve, input);

        var outcome = new MarkReservePostingFailedTransition().Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }

    private static MarkReservePostingFailedInput CreateInput()
    {
        return new MarkReservePostingFailedInput(TestClaims.Actor, TestClaims.Later)
        {
            FailureReason = "The ledger timed out",
        };
    }
}
