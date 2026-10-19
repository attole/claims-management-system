namespace ClaimsModule.Application.Results;

public class Result : IFailableResult<Result>
{
    protected Result(IReadOnlyList<Error> errors, IReadOnlyList<Warning> warnings)
    {
        Errors = errors;
        Warnings = warnings;
    }

    public IReadOnlyList<Error> Errors { get; }

    public IReadOnlyList<Warning> Warnings { get; }

    public bool IsSuccess => Errors.Count == 0;

    public static Result Success(params IReadOnlyList<Warning> warnings)
    {
        return new Result([], warnings);
    }

    public static Result Failure(IReadOnlyList<Error> errors)
    {
        return new Result(Required(errors), []);
    }

    protected static IReadOnlyList<Error> Required(IReadOnlyList<Error> errors)
    {
        if (errors.Count == 0)
        {
            throw new ArgumentException("A failure needs at least one error.", nameof(errors));
        }

        return errors;
    }
}
