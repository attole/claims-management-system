using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Flows;

public sealed class CancelReservePostingTransitionTests
{
    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Run_ForARejectedChange_CancelsThePosting()
    {
        var change = TestReserves.SubmitRejected();
        var input = CreateInput();

        var outcome = CancelReservePostingTransition.Run(change, input);

        outcome.Value.To.Should().BeOfType<ReservePostingCancelledState>();
    }

    [Fact]
    [Trait("Rule", IsReserveChangeRefused.Code)]
    public void Run_ForAnApprovedChange_ReturnsTheFindingAndKeepsThePosting()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();

        var outcome = CancelReservePostingTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeRefused.Code);
        change.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Run_FromCancelled_ReturnsNotPermitted()
    {
        var change = TestReserves.SubmitRejected();
        var input = CreateInput();
        CancelReservePostingTransition.Run(change, input);

        var outcome = CancelReservePostingTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(CancelReservePostingTransition.NotPermittedCode);
    }

    private static ReservePostingInput CreateInput()
    {
        return new ReservePostingInput();
    }
}
