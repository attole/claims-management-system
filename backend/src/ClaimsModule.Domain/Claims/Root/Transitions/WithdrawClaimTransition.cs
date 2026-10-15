using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.StateChanges.Rules;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Transitions;

public sealed record WithdrawClaimInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseTransitionInput(ActorRole, ActorId, Now, Reference);

internal sealed class WithdrawClaimTransition : ClaimBaseTransition<WithdrawClaimInput>
{
    public WithdrawClaimTransition() : base(new ClaimWithdrawnState())
    {
        AllowedFrom(new ClaimOpenState(), new ClaimUnderInvestigationState())
            .Require(input => new HasStateChangeReason(input.Reason));
    }
}
