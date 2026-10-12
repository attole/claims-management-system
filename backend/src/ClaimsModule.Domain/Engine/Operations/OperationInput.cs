namespace ClaimsModule.Domain.Engine.Operations;

public abstract record OperationInput(DateTimeOffset Now)
{
    public DateTimeOffset Now { get; } = Now != default
        ? Now
        : throw new ArgumentException("An operation input needs the time of the operation.", nameof(Now));
}
