using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.ReserveChanges.Root.Bases;

internal abstract class ReserveChangeBasePostingTransition<TInput>(ReservePostingState target)
    : Transition<ReserveChange, ReservePostingState, TInput>(target)
    where TInput : ReserveChangeBasePostingTransitionInput
{
    protected sealed override ReservePostingState Apply(ReserveChange reserve, TInput input)
    {
        reserve.PostingState = Target;
        reserve.PostingDetails = new ReservePostingDetails(input.JobId, input.FailureReason);

        return reserve.PostingState;
    }
}
