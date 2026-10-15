using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Bases;

public abstract record ClaimBaseOperationInput(DateTimeOffset Now, Guid ActorId, ClaimReferenceSnapshot Reference)
    : OperationInput(Now)
{
    public Guid ActorId { get; } = ActorId;

    public ClaimReferenceSnapshot Reference { get; } = Reference;
}
