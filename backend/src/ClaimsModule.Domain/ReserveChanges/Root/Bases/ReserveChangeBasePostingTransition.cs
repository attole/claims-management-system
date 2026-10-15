using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;

namespace ClaimsModule.Domain.ReserveChanges.Root.Bases;

internal abstract class ReserveChangeBasePostingTransition<TSelf, TInput>(ReservePostingState target)
    : Transition<TSelf, ReserveChange, ReservePostingState, TInput>(target)
    where TSelf : ReserveChangeBasePostingTransition<TSelf, TInput>, new()
    where TInput : ReserveChangeBasePostingTransitionInput
{
    protected sealed override StateChange<ReservePostingState> Apply(ReserveChange reserve, TInput input)
    {
        var change = new StateChange<ReservePostingState>(reserve.PostingState, Target);
        reserve.PostingState = Target;
        reserve.PostingDetails = new ReservePostingDetails(input.JobId, input.FailureReason);

        return change;
    }
}
