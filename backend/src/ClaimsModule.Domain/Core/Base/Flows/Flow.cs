using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Core.Base.Flows;

public abstract class Flow<TSubject, TInput, TValue, TFinding>
    where TFinding : IFinding
{
    private readonly CheckSet<TSubject, TInput, TFinding> _checks = new();

    protected virtual FlowDecision WhenBlocked => FlowDecision.Stop;

    public virtual FlowOutcome<TValue, TFinding> Execute(TSubject subject, TInput input)
    {
        var evaluated = SelectChecks(subject).Select(set => set.Evaluate(subject, input));
        var result = CheckResult<TFinding>.Combine(evaluated);

        bool isStopped = result.IsBlocked && WhenBlocked == FlowDecision.Stop;

        if (isStopped)
        {
            return FlowOutcome<TValue, TFinding>.Failure(result);
        }

        var value = Apply(subject, input, result);

        return FlowOutcome<TValue, TFinding>.Success(value, result);
    }

    protected abstract TValue Apply(TSubject subject, TInput input, CheckResult<TFinding> result);

    protected void Require(ICheck<TSubject, TFinding> check)
    {
        _checks.Require(check);
    }

    protected void Require(Func<TInput, ICheck<TSubject, TFinding>> checkFromInput)
    {
        _checks.Require(checkFromInput);
    }

    protected virtual IReadOnlyList<CheckSet<TSubject, TInput, TFinding>> SelectChecks(TSubject subject)
    {
        return [_checks];
    }
}
