using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Inputs;

public sealed record SendToPaymentClaimInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReserveSnapshot Reserves,
    ClaimReferenceSnapshot Reference) : ClaimBaseTransitionInput(ActorRole, ActorId, Now, Reference)
{
    public bool IsLiabilityConfirmed { get; init; } = false;
}
