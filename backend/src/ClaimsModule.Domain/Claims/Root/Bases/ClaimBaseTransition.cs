using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.Claims.Root.Bases;

public abstract class ClaimBaseTransition<TInput>(ClaimState target) : Transition<Claim, ClaimState, TInput>(target)
    where TInput : ClaimBaseTransitionInput
{
    private readonly EvaluateIssuesFlow _issues = new();

    protected sealed override ClaimState Apply(Claim claim, TInput input)
    {
        claim.ChangeState(Target, input.Reason, input.ActorId, input.Now);

        while (claim.State.IsPassThrough)
        {
            claim.ChangeState(claim.State.PassesTo, StateChangeReason.Empty, input.ActorId, input.Now);
        }

        _issues.Execute(claim, new EvaluateIssuesInput(input.Now, input.ActorId));

        return claim.State;
    }
}
