using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.Claims.Root.Bases;

public abstract class ClaimBaseTransition<TInput>(ClaimState target) : Transition<Claim, ClaimState, TInput>(target)
    where TInput : ClaimBaseTransitionInput
{
    protected sealed override ClaimState Apply(Claim claim, TInput input)
    {
        claim.ChangeState(Target, input.Reason, input.Note, input.ActorId, input.Now);

        return claim.State;
    }
}
