using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Operations;

internal abstract class Operation<TSelf, TSubject, TInput, TValue>
    : Flow<TSelf, TSubject, TInput, TValue, DomainFinding>
    where TSelf : Operation<TSelf, TSubject, TInput, TValue>, new()
    where TInput : OperationInput
{
    protected abstract TValue Apply(TSubject subject, TInput input);

    protected sealed override TValue Apply(TSubject subject, TInput input, CheckResult<DomainFinding> result)
    {
        return Apply(subject, input);
    }
}
