using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.ReserveChanges.Root.Bases;

public abstract class ReserveChangeBaseTransition<TInput>(ReserveApprovalState target)
    : Transition<ReserveChange, ReserveApprovalState, TInput>(target)
    where TInput : ReserveChangeBaseTransitionInput
{
    protected sealed override ReserveApprovalState Apply(ReserveChange reserve, TInput input)
    {
        decimal approvedBalance = input.Snapshot.ReserveApprovedBalance;
        reserve.ChangeApprovalState(Target, input.Reason, input.ActorId, approvedBalance, input.Now);

        return reserve.ApprovalState;
    }
}
