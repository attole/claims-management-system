using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Operations;

public abstract class CreationOperation<TInput, TValue> : Operation<NoSubject, TInput, TValue>
    where TInput : OperationInput
{
    public FlowOutcome<TValue, DomainFinding> Execute(TInput input)
    {
        return Execute(NoSubject.Value, input);
    }

    protected abstract TValue Apply(TInput input);

    protected sealed override TValue Apply(NoSubject subject, TInput input)
    {
        return Apply(input);
    }
}
