using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public sealed record SampleMoveClaimInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseTransitionInput(ActorRole, ActorId, Now, Reference);

internal sealed class SampleMoveClaimTransition<TTarget>
    : ClaimBaseTransition<SampleMoveClaimTransition<TTarget>, SampleMoveClaimInput>
    where TTarget : ClaimState, new()
{
    public SampleMoveClaimTransition() : base(new TTarget())
    {
        AllowedFrom(new ClaimDraftState());
    }
}
