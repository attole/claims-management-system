using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Operations.Creation;

public abstract class CreationOperation<TInput, TValue> : Operation<NoSubject, TInput, TValue>
    where TInput : OperationInput
{
    public FlowOutcome<TValue, DomainFinding> Execute(TInput input)
    {
        return Execute(NoSubject.Value, input);
    }

    protected abstract TValue Apply(TInput input);

    protected virtual void AfterCreate(TValue value, TInput input)
    {
    }

    protected sealed override TValue Apply(NoSubject subject, TInput input)
    {
        var value = Apply(input);
        AfterCreate(value, input);

        return value;
    }
}
