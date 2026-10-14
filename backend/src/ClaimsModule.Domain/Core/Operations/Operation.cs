using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Operations;

public abstract class Operation<TSubject, TInput, TValue> : Flow<TSubject, TInput, TValue, DomainFinding>
    where TInput : OperationInput
{
    protected abstract TValue Apply(TSubject subject, TInput input);

    protected sealed override TValue Apply(TSubject subject, TInput input, CheckResult<DomainFinding> result)
    {
        return Apply(subject, input);
    }
}
