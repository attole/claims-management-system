using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.StateChanges.Rules;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Domain.Roles.Rules;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class ReopenClaimTransition : ClaimBaseTransition<ReopenClaimTransition, ReopenClaimInput>
{
    public ReopenClaimTransition() : base(new ClaimReopenedState())
    {
        AllowedFrom(new ClaimClosedState())
            .Require(input => new HasMinimumRole(input.ActorRole, new UserSupervisorRole()))
            .Require(input => new HasStateChangeReason(input.Reason));
    }
}
