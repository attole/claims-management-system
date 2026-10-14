using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Markers;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Transitions;

public abstract class Transition<TSubject, TState, TInput> : Operation<TSubject, TInput, TState>
    where TSubject : IStateful<TState>
    where TState : class, IMarked, IEquatable<TState>
    where TInput : OperationInput
{
    private readonly Dictionary<TState, RuleSet<TSubject, TInput>> _rulesBySource = [];

    protected Transition(TState target)
    {
        Target = target;
    }

    public TState Target { get; }

    public IReadOnlyCollection<TState> Sources => _rulesBySource.Keys;

    protected sealed override FlowDecision WhenPreconditionFails => FlowDecision.Stop;

    protected sealed override FlowDecision WhenBlocked => FlowDecision.Stop;

    protected AllowedSourcesBuilder<TSubject, TState, TInput> AllowedFrom(params TState[] sources)
    {
        return new AllowedSourcesBuilder<TSubject, TState, TInput>(_rulesBySource).AllowedFrom(sources);
    }

    protected sealed override IReadOnlyList<DomainFinding> CheckPreconditions(TSubject subject, TInput input)
    {
        var current = subject.State;

        bool isDeclaredSource = _rulesBySource.ContainsKey(current);

        return isDeclaredSource
            ? []
            : [new TransitionFinding(current.Name, Target.Name)];
    }

    protected sealed override IReadOnlyList<CheckSet<TSubject, TInput, DomainFinding>> SelectChecks(TSubject subject)
    {
        var operationRules = base.SelectChecks(subject);
        var sourceRules = _rulesBySource[subject.State];

        return [.. operationRules, sourceRules];
    }
}
