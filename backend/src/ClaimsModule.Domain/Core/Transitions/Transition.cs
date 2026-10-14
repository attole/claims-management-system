using ClaimsModule.Domain.Common.Markers;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Transitions;

public abstract class Transition<TSubject, TState, TInput> : Operation<TSubject, TInput, Success>
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

    protected abstract TState CurrentState(TSubject subject);

    protected abstract void Move(TSubject subject, TState state, TInput input);

    protected virtual void AfterMove(TSubject subject, TInput input)
    {
    }

    protected override Success Apply(TSubject subject, TInput input)
    {
        return Success.Value;
    }

    protected sealed override IReadOnlyList<DomainFinding> CheckPreconditions(TSubject subject, TInput input)
    {
        var current = CurrentState(subject);

        bool isDeclaredSource = _rulesBySource.ContainsKey(current);

        return isDeclaredSource
            ? []
            : [TransitionFinding.Create(current.Name, Target.Name)];
    }

    protected sealed override IReadOnlyList<CheckSet<TSubject, TInput, DomainFinding>> SelectChecks(TSubject subject)
    {
        var operationRules = base.SelectChecks(subject);
        var sourceRules = _rulesBySource[CurrentState(subject)];

        return [.. operationRules, sourceRules];
    }

    protected sealed override void AfterApply(TSubject subject, TInput input)
    {
        Move(subject, Target, input);
        AfterMove(subject, input);
    }
}
