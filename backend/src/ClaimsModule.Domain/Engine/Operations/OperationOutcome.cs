using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Engine.Operations;

public sealed class OperationOutcome<TValue>
{
    private readonly TValue _value;

    private OperationOutcome(
        bool isSuccess,
        TValue value,
        IReadOnlyList<DomainError> errors,
        IReadOnlyList<DomainError> warnings)
    {
        IsSuccess = isSuccess;
        _value = value;
        Errors = errors;
        Warnings = warnings;
    }

    public bool IsSuccess { get; }

    public IReadOnlyList<DomainError> Errors { get; }

    public IReadOnlyList<DomainError> Warnings { get; }

    public TValue Value => IsSuccess ? _value : throw new InvalidOperationException("A failed outcome has no value.");

    public static OperationOutcome<TValue> Success(TValue value, IReadOnlyList<DomainError> warnings)
    {
        return new OperationOutcome<TValue>(true, value, [], warnings);
    }

    public static OperationOutcome<TValue> Failure(
        IReadOnlyList<DomainError> errors,
        IReadOnlyList<DomainError> warnings)
    {
        return new OperationOutcome<TValue>(false, default!, errors, warnings);
    }
}
