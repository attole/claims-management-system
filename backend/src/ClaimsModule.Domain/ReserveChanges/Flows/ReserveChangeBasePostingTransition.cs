using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal abstract class ReserveChangeBasePostingTransition<TSelf>(ReservePostingState target)
    : Transition<TSelf, ReserveChange, ReservePostingState, ReservePostingInput>(target)
    where TSelf : ReserveChangeBasePostingTransition<TSelf>, new()
{
    protected sealed override void ApplyMove(
        ReserveChange change,
        ReservePostingInput input,
        Move<ReservePostingState> move)
    {
        change.PostingState = move.To;
        change.PostingDetails = new ReservePostingDetails(input.JobId, input.FailureReason);
    }
}
