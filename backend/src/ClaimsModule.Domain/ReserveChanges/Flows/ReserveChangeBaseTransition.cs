using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal abstract class ReserveChangeBaseTransition<TSelf, TInput>(ReserveApprovalState target)
    : Transition<TSelf, ReserveChange, ReserveApprovalState, TInput>(target)
    where TSelf : ReserveChangeBaseTransition<TSelf, TInput>, new()
    where TInput : ReserveDecisionInput
{
    protected sealed override void ApplyMove(ReserveChange change, TInput input, Move<ReserveApprovalState> move)
    {
        var reviewedAt = InputTime.NotBefore(input.Now, change.SubmittedAt);
        change.Review = new ReserveReview(input.ReviewerId, reviewedAt, input.Reason);
        change.ApprovalState = move.To;
    }
}
