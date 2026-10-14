using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root;

public abstract record ClaimBaseInput(DateTimeOffset Now, Guid ActorId) : OperationInput(Now)
{
    public Guid ActorId { get; } = ActorId;
}
