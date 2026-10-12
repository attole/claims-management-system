using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Common.Markers;
using ClaimsModule.Domain.Engine.Operations;
using ClaimsModule.Domain.Engine.Rules;

namespace ClaimsModule.Domain.Engine.Transitions;

public abstract class Transition<TSubject, TState, TInput> : Operation<TSubject, TInput, Success>
    where TState : class, IMarked
    where TInput : OperationInput
{
    private readonly Dictionary<TState, RuleSet<TSubject, TInput>> _groups = [];

    protected Transition(TState target)
    {
        Target = target;
    }

    public TState Target { get; }

    public IReadOnlyCollection<TState> Sources => _groups.Keys;

    protected virtual string NotPermittedCode => TransitionNotPermitted.DefaultCode;

    protected TransitionBuilder<TSubject, TState, TInput> From(params TState[] sources)
    {
        return new TransitionBuilder<TSubject, TState, TInput>(_groups).From(sources);
    }

    protected abstract TState CurrentState(TSubject subject);

    protected abstract void Move(TSubject subject, TState state);

    private protected override IReadOnlyList<DomainError> CheckPreconditions(TSubject subject)
    {
        TState current = CurrentState(subject);

        return _groups.ContainsKey(current)
            ? []
            : [TransitionNotPermitted.Create(NotPermittedCode, current.Name, Target.Name)];
    }

    private protected override IReadOnlyList<RuleSet<TSubject, TInput>> SelectRules(TSubject subject)
    {
        return [.. base.SelectRules(subject), _groups[CurrentState(subject)]];
    }

    private protected override void AfterApply(TSubject subject)
    {
        Move(subject, Target);
    }
}
