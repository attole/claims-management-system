using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Flows;

public sealed class Outcome<TValue>
{
    private readonly TValue _value;

    private Outcome(bool isSuccess, TValue value, CheckResult result)
    {
        IsSuccess = isSuccess;
        _value = value;
        Result = result;
    }

    public bool IsSuccess { get; }

    public TValue Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Only a succeeded outcome has a value.");

    public IReadOnlyList<Finding> CriticalFindings => Result.CriticalFindings;

    public IReadOnlyList<Finding> WarningFindings => Result.WarningFindings;

    internal CheckResult Result { get; }

    internal static Outcome<TValue> Success(TValue value, CheckResult result)
    {
        return new Outcome<TValue>(true, value, result);
    }

    internal static Outcome<TValue> Failure(CheckResult result)
    {
        return new Outcome<TValue>(false, default!, result);
    }

    internal Outcome<TValue> OnSuccess(Action<TValue> then)
    {
        if (IsSuccess)
        {
            then(_value);
        }

        return this;
    }

    internal Outcome<TValue> OnSuccess(Action then)
    {
        if (IsSuccess)
        {
            then();
        }

        return this;
    }

    internal Outcome<TValue> WithFindings(CheckResult result)
    {
        var findings = CheckResult.Combine([result, Result]);

        return new Outcome<TValue>(IsSuccess, _value, findings);
    }
}
