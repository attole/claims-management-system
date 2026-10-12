using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Rules;

namespace ClaimsModule.Domain.Engine.Operations;

public abstract class Operation<TSubject, TInput, TValue>
    where TInput : OperationInput
{
    private readonly RuleSet<TSubject, TInput> _rules = new();

    public OperationOutcome<TValue> Execute(TSubject subject, TInput input)
    {
        IReadOnlyList<DomainError> preconditionErrors = CheckPreconditions(subject);

        if (preconditionErrors.Count > 0)
        {
            return OperationOutcome<TValue>.Failure(preconditionErrors, []);
        }

        IEnumerable<RuleResult> results = SelectRules(subject).Select(set => set.Evaluate(subject, input));
        RuleResult result = RuleResult.Combine(results);

        if (result.Errors.Count > 0)
        {
            return OperationOutcome<TValue>.Failure(result.Errors, result.Warnings);
        }

        TValue value = Apply(subject, input);
        AfterApply(subject);

        return OperationOutcome<TValue>.Success(value, result.Warnings);
    }

    protected void Require(IRule<TSubject> rule)
    {
        _rules.Add(rule);
    }

    protected void Require(Func<TInput, IRule<TSubject>> factory)
    {
        _rules.Add(factory);
    }

    protected abstract TValue Apply(TSubject subject, TInput input);

    private protected virtual IReadOnlyList<DomainError> CheckPreconditions(TSubject subject)
    {
        return [];
    }

    private protected virtual IReadOnlyList<RuleSet<TSubject, TInput>> SelectRules(TSubject subject)
    {
        return [_rules];
    }

    private protected virtual void AfterApply(TSubject subject)
    {
    }
}
