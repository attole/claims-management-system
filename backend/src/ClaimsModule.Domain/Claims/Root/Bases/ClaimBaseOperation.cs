using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Bases;

public abstract class ClaimBaseOperation<TInput, TValue> : Operation<Claim, TInput, TValue>
    where TInput : ClaimBaseOperationInput
{
    private readonly EvaluateIssuesFlow _issues = new();

    protected sealed override void AfterApply(Claim claim, TInput input)
    {
        _issues.Execute(claim, new EvaluateIssuesInput(input.Now, input.ActorId));
    }
}
