using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Claims.StateChanges.Rules;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Transitions;

public sealed record ReopenClaimInput(UserRole ActorRole, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseTransitionInput(ActorRole, ActorId, Now);

public sealed class ReopenClaimTransition : ClaimBaseTransition<ReopenClaimInput>
{
    public ReopenClaimTransition()
        : base(new ClaimReopenedState())
    {
        AllowedFrom(new ClaimClosedState())
            .Require(input => new HasPermittedRole(input.ActorRole, UserRole.Supervisor))
            .Require(input => new HasStateChangeReason(input.Reason));
    }
}
