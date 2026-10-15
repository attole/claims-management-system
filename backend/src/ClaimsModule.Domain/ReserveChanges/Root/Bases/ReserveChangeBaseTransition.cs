using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.ReserveChanges.Root.Bases;

internal abstract class ReserveChangeBaseTransition<TInput>(ReserveApprovalState target)
    : Transition<ReserveChange, ReserveApprovalState, TInput>(target)
    where TInput : ReserveChangeBaseTransitionInput
{
    protected sealed override ReserveApprovalState Apply(ReserveChange reserve, TInput input)
    {
        decimal approvedBalance = input.Snapshot.ReserveApprovedBalance;
        reserve.Review = new ReserveReview(input.ActorId, input.Now, input.Reason);
        reserve.ApprovalState = Target;

        if (Target.IsApproved)
        {
            reserve.Balances = new ReserveBalances(approvedBalance, approvedBalance + reserve.Details.Amount);
        }

        return reserve.ApprovalState;
    }
}
