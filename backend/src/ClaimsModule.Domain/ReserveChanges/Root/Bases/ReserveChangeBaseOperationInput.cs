using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.ReserveChanges.Root.Bases;

public abstract record ReserveChangeBaseOperationInput(DateTimeOffset Now, Guid ActorId) : OperationInput(Now)
{
    public Guid ActorId { get; } = ActorId;
}
