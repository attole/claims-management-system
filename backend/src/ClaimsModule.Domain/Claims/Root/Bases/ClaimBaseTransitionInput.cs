using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Bases;

public abstract record ClaimBaseTransitionInput(UserRole ActorRole, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId)
{
    public StateChangeReason Reason { get; init; } = StateChangeReason.Empty;
}
