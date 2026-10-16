namespace ClaimsModule.Domain.Core.Rules;

internal sealed class RuleSet<TSubject, TInput>
{
    private readonly List<Func<TInput, IRule<TSubject>>> _ruleFactories = [];

    public CheckResult Check(TSubject subject, TInput input)
    {
        var results = _ruleFactories.Select(ruleFactory => ruleFactory(input).Check(subject));

        return CheckResult.Combine(results);
    }

    public RuleSet<TSubject, TInput> Require(IRule<TSubject> rule)
    {
        _ruleFactories.Add(_ => rule);

        return this;
    }

    public RuleSet<TSubject, TInput> Require(Func<TInput, IRule<TSubject>> ruleFactory)
    {
        _ruleFactories.Add(ruleFactory);

        return this;
    }
}
