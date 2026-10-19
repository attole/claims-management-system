namespace ClaimsModule.Application.Results;

public interface IFailableResult<TResult>
    where TResult : IFailableResult<TResult>
{
    static abstract TResult Failure(IReadOnlyList<Error> errors);
}
