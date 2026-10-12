namespace ClaimsModule.Domain.Engine.Rules;

internal sealed class RuleSet<TSubject, TInput>
{
    private readonly List<Func<TInput, IRule<TSubject>>> _factories = [];

    public void Add(IRule<TSubject> rule)
    {
        _factories.Add(_ => rule);
    }

    public void Add(Func<TInput, IRule<TSubject>> factory)
    {
        _factories.Add(factory);
    }

    public RuleResult Evaluate(TSubject subject, TInput input)
    {
        IEnumerable<RuleResult> results = _factories.Select(factory => factory(input).Check(subject));

        return RuleResult.Combine(results);
    }
}
