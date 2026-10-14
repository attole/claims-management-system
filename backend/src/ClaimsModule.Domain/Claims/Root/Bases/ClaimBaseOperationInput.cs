using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Bases;

public abstract record ClaimBaseOperationInput(DateTimeOffset Now, Guid ActorId) : OperationInput(Now)
{
    public Guid ActorId { get; } = ActorId;
}
