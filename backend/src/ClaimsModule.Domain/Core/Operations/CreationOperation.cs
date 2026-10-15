using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Operations;

internal abstract class CreationOperation<TSelf, TInput, TValue> : Operation<TSelf, NoSubject, TInput, TValue>
    where TSelf : CreationOperation<TSelf, TInput, TValue>, new()
    where TInput : OperationInput
{
    public static FlowOutcome<TValue, DomainFinding> Execute(TInput input)
    {
        return Execute(NoSubject.Value, input);
    }

    protected abstract TValue Apply(TInput input);

    protected sealed override TValue Apply(NoSubject subject, TInput input)
    {
        return Apply(input);
    }
}
