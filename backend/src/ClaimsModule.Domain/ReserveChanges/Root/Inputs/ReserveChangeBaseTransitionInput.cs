using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.ReserveChanges.Root.Inputs;

public abstract record ReserveChangeBaseTransitionInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ReserveSnapshot Snapshot) : ReserveChangeBaseOperationInput(Now, ActorId)
{
    public string Reason { get; init; } = string.Empty;
}
