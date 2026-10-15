using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Reserves.Root.Bases;

public abstract record ReserveBaseOperationInput(DateTimeOffset Now, Guid ActorId) : OperationInput(Now)
{
    public Guid ActorId { get; } = ActorId;
}
