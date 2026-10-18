using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal abstract class ReserveHistoryBaseTransition<TSelf, TInput>(ReserveApprovalState target)
    : Transition<TSelf, ReserveHistory, ReserveApprovalState, TInput>(target)
    where TSelf : ReserveHistoryBaseTransition<TSelf, TInput>, new()
    where TInput : RowDecisionInput
{
    protected sealed override void ApplyMove(ReserveHistory history, TInput input, Move<ReserveApprovalState> move)
    {
        var reviewedAt = InputTime.NotBefore(input.Now, history.SubmittedAt);
        history.Review = new ReserveReview(input.ReviewerId, reviewedAt, input.Reason);
        history.ApprovalState = move.To;
    }
}
