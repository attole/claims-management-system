using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Inputs;

public abstract record ClaimBaseOperationInput(DateTimeOffset Now, Guid ActorId, ClaimReferenceSnapshot Reference)
    : OperationInput(Now, ActorId)
{
    public ClaimReferenceSnapshot Reference { get; } = Reference;
}
