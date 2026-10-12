using ClaimsModule.Domain.Common.Markers;
using ClaimsModule.Domain.Engine.Operations;
using ClaimsModule.Domain.Engine.Rules;

namespace ClaimsModule.Domain.Engine.Transitions;

public sealed class TransitionBuilder<TSubject, TState, TInput>
    where TState : class, IMarked
    where TInput : OperationInput
{
    private readonly Dictionary<TState, RuleSet<TSubject, TInput>> _groups;
    private RuleSet<TSubject, TInput> _current = new();

    internal TransitionBuilder(Dictionary<TState, RuleSet<TSubject, TInput>> groups)
    {
        _groups = groups;
    }

    public TransitionBuilder<TSubject, TState, TInput> From(params TState[] sources)
    {
        _current = new RuleSet<TSubject, TInput>();

        foreach (TState source in sources)
        {
            if (!_groups.TryAdd(source, _current))
            {
                throw new InvalidOperationException($"Source {source.Name} is declared twice.");
            }
        }

        return this;
    }

    public TransitionBuilder<TSubject, TState, TInput> Require(IRule<TSubject> rule)
    {
        _current.Add(rule);

        return this;
    }

    public TransitionBuilder<TSubject, TState, TInput> Require(Func<TInput, IRule<TSubject>> factory)
    {
        _current.Add(factory);

        return this;
    }
}
