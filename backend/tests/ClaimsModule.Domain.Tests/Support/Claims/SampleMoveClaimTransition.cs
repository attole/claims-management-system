using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public sealed record SampleMoveClaimInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseTransitionInput(ActorRole, ActorId, Now, Reference);

internal sealed class SampleMoveClaimTransition : ClaimBaseTransition<SampleMoveClaimInput>
{
    public SampleMoveClaimTransition(ClaimState target) : base(target)
    {
        AllowedFrom(new ClaimDraftState());
    }
}
