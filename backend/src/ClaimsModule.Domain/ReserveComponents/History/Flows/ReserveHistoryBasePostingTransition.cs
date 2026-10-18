using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal abstract class ReserveHistoryBasePostingTransition<TSelf>(ReservePostingState target)
    : Transition<TSelf, ReserveHistory, ReservePostingState, RowPostingInput>(target)
    where TSelf : ReserveHistoryBasePostingTransition<TSelf>, new()
{
    protected sealed override void ApplyMove(
        ReserveHistory history,
        RowPostingInput input,
        Move<ReservePostingState> move)
    {
        history.PostingState = move.To;
        history.PostingDetails = new ReservePostingDetails(input.JobId, input.FailureReason);
    }
}
