using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class InvestigateClaimTransition
    : ClaimBaseTransition<InvestigateClaimTransition, InvestigateClaimInput>
{
    public InvestigateClaimTransition() : base(new ClaimUnderInvestigationState())
    {
        AllowedFrom(new ClaimOpenState())
            .Require(input => new HasPermittedRole(input.ActorRole, UserRole.Handler, UserRole.Supervisor));
    }
}
