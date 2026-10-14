using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.Claims.Root;

public abstract class ClaimBaseTransition<TInput>(ClaimState target) : Transition<Claim, ClaimState, TInput>(target)
    where TInput : ClaimBaseInput
{
    private readonly EvaluateIssuesFlow _issues = new();

    protected sealed override ClaimState CurrentState(Claim claim)
    {
        return claim.State;
    }

    protected sealed override void Move(Claim claim, ClaimState state, TInput input)
    {
        claim.ChangeState(state, input.ActorId, input.Now);
    }

    protected sealed override void AfterMove(Claim claim, TInput input)
    {
        _issues.Execute(claim, new EvaluateIssuesInput(input.Now, input.ActorId));
    }
}
