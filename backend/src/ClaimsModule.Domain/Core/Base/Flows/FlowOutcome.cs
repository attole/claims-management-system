using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Core.Base.Flows;

public sealed class FlowOutcome<TValue, TFinding>
    where TFinding : IFinding
{
    private readonly TValue _value;

    private FlowOutcome(FlowStatus status, TValue value, CheckResult<TFinding> result)
    {
        Status = status;
        _value = value;
        Result = result;
    }

    public FlowStatus Status { get; }

    public CheckResult<TFinding> Result { get; }

    public bool IsSuccess => Status == FlowStatus.Succeeded;

    public IReadOnlyList<TFinding> Blocking => Result.Blocking;

    public IReadOnlyList<TFinding> Advisory => Result.Advisory;

    public TValue Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Only a succeeded outcome has a value.");

    public static FlowOutcome<TValue, TFinding> Success(TValue value, CheckResult<TFinding> result)
    {
        return new FlowOutcome<TValue, TFinding>(FlowStatus.Succeeded, value, result);
    }

    public static FlowOutcome<TValue, TFinding> Failure(CheckResult<TFinding> result)
    {
        return new FlowOutcome<TValue, TFinding>(FlowStatus.Failed, default!, result);
    }
}
