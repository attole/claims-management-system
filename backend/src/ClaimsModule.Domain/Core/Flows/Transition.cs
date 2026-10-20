using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Markers;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Flows;

internal abstract class Transition<TSelf, TSubject, TState, TInput>
    : Operation<TSelf, TSubject, TInput, Move<TState>>, ITransition<TState>
    where TSelf : Transition<TSelf, TSubject, TState, TInput>, new()
    where TSubject : IStateful<TState>
    where TState : class, IMarked, IEquatable<TState>
{
    public const string NotPermittedCode = TransitionCodes.NotPermitted;

    private readonly Dictionary<TState, RuleSet<TSubject, TInput>> _rulesBySource = [];
    private readonly TState _target;

    protected Transition(TState target)
    {
        _target = target;
    }

    public TState Target => _target;

    public IReadOnlyCollection<TState> Sources => _rulesBySource.Keys;

    protected RuleSet<TSubject, TInput> AllowedFrom(params TState[] sources)
    {
        if (sources.Length == 0)
        {
            throw new ArgumentException("Declare at least one source state.", nameof(sources));
        }

        var rules = new RuleSet<TSubject, TInput>();

        foreach (var source in sources)
        {
            if (!_rulesBySource.TryAdd(source, rules))
            {
                throw new InvalidOperationException($"Source {source.Name} is declared twice.");
            }
        }

        return rules;
    }

    protected sealed override CheckResult Check(TSubject subject, TInput input)
    {
        var current = subject.State;
        bool isDeclaredSource = _rulesBySource.ContainsKey(current);

        if (!isDeclaredSource)
        {
            var message = $"Transition from {current.Name} to {_target.Name} is not permitted.";

            return CheckResult.Found(new Finding(NotPermittedCode, message));
        }

        var operationResult = base.Check(subject, input);
        var sourceResult = _rulesBySource[current].Check(subject, input);

        return CheckResult.Combine([operationResult, sourceResult]);
    }

    protected sealed override Outcome<Move<TState>> Apply(TSubject subject, TInput input)
    {
        var move = new Move<TState>(subject.State, _target);
        ApplyMove(subject, input, move);

        return Done(move);
    }

    protected abstract void ApplyMove(TSubject subject, TInput input, Move<TState> move);
}
