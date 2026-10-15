using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Bases;

public abstract record ClaimBaseTransitionInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference)
{
    public StateChangeReason Reason { get; init; } = StateChangeReason.Empty;

    public string Note { get; init; } = string.Empty;
}
