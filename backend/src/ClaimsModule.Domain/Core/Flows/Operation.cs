using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Flows;

internal abstract class Operation<TSelf, TSubject, TInput, TValue>
    where TSelf : Operation<TSelf, TSubject, TInput, TValue>, new()
{
    private readonly RuleSet<TSubject, TInput> _rules = new();

    protected static TSelf Instance { get; } = new();

    public static Outcome<TValue> Run(TSubject subject, TInput input)
    {
        var result = Instance.Check(subject, input);

        if (result.HasCriticalFindings)
        {
            return Outcome<TValue>.Failure(result);
        }

        return Instance.Apply(subject, input).WithFindings(result);
    }

    protected virtual CheckResult Check(TSubject subject, TInput input)
    {
        return _rules.Check(subject, input);
    }

    protected abstract Outcome<TValue> Apply(TSubject subject, TInput input);

    protected static Outcome<TValue> Done(TValue value)
    {
        return Outcome<TValue>.Success(value, CheckResult.Pass);
    }

    protected void Require(IRule<TSubject> rule)
    {
        _rules.Require(rule);
    }

    protected void Require(Func<TInput, IRule<TSubject>> ruleFactory)
    {
        _rules.Require(ruleFactory);
    }
}
