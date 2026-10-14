using ClaimsModule.Domain.Common.Markers;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Transitions;

public sealed class AllowedSourcesBuilder<TSubject, TState, TInput>
    where TState : class, IMarked, IEquatable<TState>
    where TInput : OperationInput
{
    private readonly Dictionary<TState, RuleSet<TSubject, TInput>> _rulesBySource;
    private RuleSet<TSubject, TInput> _current = new();

    internal AllowedSourcesBuilder(Dictionary<TState, RuleSet<TSubject, TInput>> rulesBySource)
    {
        _rulesBySource = rulesBySource;
    }

    public AllowedSourcesBuilder<TSubject, TState, TInput> AllowedFrom(params TState[] sources)
    {
        if (sources.Length == 0)
        {
            throw new ArgumentException("Declare at least one source state.", nameof(sources));
        }

        _current = new RuleSet<TSubject, TInput>();

        foreach (var source in sources)
        {
            if (!_rulesBySource.TryAdd(source, _current))
            {
                throw new InvalidOperationException($"Source {source.Name} is declared twice.");
            }
        }

        return this;
    }

    public AllowedSourcesBuilder<TSubject, TState, TInput> Require(IRule<TSubject> rule)
    {
        _current.Require(rule);

        return this;
    }

    public AllowedSourcesBuilder<TSubject, TState, TInput> Require(Func<TInput, IRule<TSubject>> ruleFromInput)
    {
        _current.Require(ruleFromInput);

        return this;
    }
}
