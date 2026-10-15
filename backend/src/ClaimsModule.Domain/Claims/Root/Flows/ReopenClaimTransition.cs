using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Claims.StateChanges.Rules;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class ReopenClaimTransition : ClaimBaseTransition<ReopenClaimTransition, ReopenClaimInput>
{
    public ReopenClaimTransition() : base(new ClaimReopenedState())
    {
        AllowedFrom(new ClaimClosedState())
            .Require(input => new HasPermittedRole(input.ActorRole, UserRole.Supervisor))
            .Require(input => new HasStateChangeReason(input.Reason));
    }
}
