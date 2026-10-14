using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Core.Base.Flows;

public abstract class Flow<TSubject, TInput, TValue, TFinding>
    where TFinding : IFinding
{
    private readonly CheckSet<TSubject, TInput, TFinding> _checks = new();

    protected virtual FlowDecision WhenPreconditionFails => FlowDecision.Stop;

    protected virtual FlowDecision WhenBlocked => FlowDecision.Stop;

    public FlowOutcome<TValue, TFinding> Execute(TSubject subject, TInput input)
    {
        var preconditionResult = new CheckResult<TFinding>(CheckPreconditions(subject, input));

        if (IsEndingRun(preconditionResult, WhenPreconditionFails))
        {
            return EndRun(preconditionResult, WhenPreconditionFails);
        }

        var evaluated = SelectChecks(subject).Select(set => set.Evaluate(subject, input));
        var checkResult = CheckResult<TFinding>.Combine(evaluated);
        var result = CheckResult<TFinding>.Combine([preconditionResult, checkResult]);

        if (IsEndingRun(checkResult, WhenBlocked))
        {
            return EndRun(result, WhenBlocked);
        }

        var value = Apply(subject, input, result);
        AfterApply(subject, input);

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

    protected virtual IReadOnlyList<TFinding> CheckPreconditions(TSubject subject, TInput input)
    {
        return [];
    }

    protected virtual void AfterApply(TSubject subject, TInput input)
    {
    }

    private static bool IsEndingRun(CheckResult<TFinding> result, FlowDecision decision)
    {
        return result.IsBlocked && decision != FlowDecision.Continue;
    }

    private static FlowOutcome<TValue, TFinding> EndRun(CheckResult<TFinding> result, FlowDecision decision)
    {
        return decision == FlowDecision.Skip
            ? FlowOutcome<TValue, TFinding>.Skipped(result)
            : FlowOutcome<TValue, TFinding>.Failure(result);
    }
}
