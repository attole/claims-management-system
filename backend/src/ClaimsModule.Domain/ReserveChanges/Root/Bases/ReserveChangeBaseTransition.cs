using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;

namespace ClaimsModule.Domain.ReserveChanges.Root.Bases;

internal abstract class ReserveChangeBaseTransition<TSelf, TInput>(ReserveApprovalState target)
    : Transition<TSelf, ReserveChange, ReserveApprovalState, TInput>(target)
    where TSelf : ReserveChangeBaseTransition<TSelf, TInput>, new()
    where TInput : ReserveChangeBaseTransitionInput
{
    protected sealed override StateChange<ReserveApprovalState> Apply(ReserveChange reserve, TInput input)
    {
        decimal approvedBalance = input.Snapshot.ReserveApprovedBalance;
        var change = new StateChange<ReserveApprovalState>(reserve.ApprovalState, Target);
        reserve.Review = new ReserveReview(input.ActorId, input.Now, input.Reason);
        reserve.ApprovalState = Target;

        if (Target.IsApproved)
        {
            reserve.Balances = new ReserveBalances(approvedBalance, approvedBalance + reserve.Details.Amount);
        }

        return change;
    }
}
