namespace ClaimsModule.Application.Results;

public sealed class Result<TValue> : Result, IFailableResult<Result<TValue>>
{
    private readonly TValue _value;

    private Result(TValue value, IReadOnlyList<Error> errors, IReadOnlyList<Warning> warnings)
        : base(errors, warnings)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Only a succeeded result has a value.");

    public static Result<TValue> Success(TValue value, params IReadOnlyList<Warning> warnings)
    {
        return new Result<TValue>(value, [], warnings);
    }

    public static new Result<TValue> Failure(IReadOnlyList<Error> errors)
    {
        return new Result<TValue>(default!, Required(errors), []);
    }

    public Result<TOther> Map<TOther>(Func<TValue, TOther> map)
    {
        return IsSuccess ? Result<TOther>.Success(map(Value), Warnings) : Result<TOther>.Failure(Errors);
    }
}
