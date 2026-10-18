using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal abstract class ReserveHistoryBaseApprovalTransition<TSelf, TInput>(ReserveApprovalState target)
    : Transition<TSelf, ReserveHistory, ReserveApprovalState, TInput>(target)
    where TSelf : ReserveHistoryBaseApprovalTransition<TSelf, TInput>, new()
    where TInput : RowApprovalInput
{
    protected sealed override void ApplyMove(ReserveHistory history, TInput input, Move<ReserveApprovalState> move)
    {
        var reviewedAt = InputTime.NotBefore(input.Now, history.SubmittedAt);
        decimal approvedBalance = input.ReserveApprovedBalance;
        history.Review = new ReserveReview(input.ReviewerId, reviewedAt, input.Reason);
        history.ApprovalState = move.To;
        history.Balances = new ReserveBalances(approvedBalance, history.Details.BalanceAfter(approvedBalance));
    }
}
