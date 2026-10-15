using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Markers;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Transitions;

internal abstract class Transition<TSelf, TSubject, TState, TInput>
    : Operation<TSelf, TSubject, TInput, StateChange<TState>>
    where TSelf : Transition<TSelf, TSubject, TState, TInput>, new()
    where TSubject : IStateful<TState>
    where TState : class, IMarked, IEquatable<TState>
    where TInput : OperationInput
{
    private readonly Dictionary<TState, RuleSet<TSubject, TInput>> _rulesBySource = [];
    private readonly TState _target;

    protected Transition(TState target)
    {
        _target = target;
    }

    public static TState Target => Instance._target;

    public static IReadOnlyCollection<TState> Sources => Instance._rulesBySource.Keys;

    protected sealed override FlowDecision WhenBlocked => FlowDecision.Stop;

    protected sealed override FlowOutcome<StateChange<TState>, DomainFinding> Run(TSubject subject, TInput input)
    {
        var current = subject.State;
        bool isDeclaredSource = _rulesBySource.ContainsKey(current);

        if (isDeclaredSource)
        {
            return base.Run(subject, input);
        }

        var refusal = CheckResult<DomainFinding>.Found(new TransitionFinding(current.Name, _target.Name));

        return FlowOutcome<StateChange<TState>, DomainFinding>.Failure(refusal);
    }

    protected AllowedSourcesBuilder<TSubject, TState, TInput> AllowedFrom(params TState[] sources)
    {
        return new AllowedSourcesBuilder<TSubject, TState, TInput>(_rulesBySource).AllowedFrom(sources);
    }

    protected sealed override IReadOnlyList<CheckSet<TSubject, TInput, DomainFinding>> SelectChecks(TSubject subject)
    {
        var operationRules = base.SelectChecks(subject);
        var sourceRules = _rulesBySource[subject.State];

        return [.. operationRules, sourceRules];
    }
}
