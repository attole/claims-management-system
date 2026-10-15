namespace ClaimsModule.Domain.ReserveChanges.Root.Bases;

public abstract record ReserveChangeBasePostingTransitionInput(Guid ActorId, DateTimeOffset Now)
    : ReserveChangeBaseOperationInput(Now, ActorId)
{
    public string JobId { get; init; } = string.Empty;

    public string FailureReason { get; init; } = string.Empty;
}
