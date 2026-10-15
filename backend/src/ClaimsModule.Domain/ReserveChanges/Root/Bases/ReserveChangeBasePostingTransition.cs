using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.ReserveChanges.Root.Bases;

public abstract class ReserveChangeBasePostingTransition<TInput>(ReservePostingState target)
    : Transition<ReserveChange, ReservePostingState, TInput>(target)
    where TInput : ReserveChangeBasePostingTransitionInput
{
    protected sealed override ReservePostingState Apply(ReserveChange reserve, TInput input)
    {
        var details = new ReservePostingDetails(input.JobId, input.FailureReason);
        reserve.ChangePostingState(Target, details, input.ActorId, input.Now);

        return reserve.PostingState;
    }
}
