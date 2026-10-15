using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Transitions;

public sealed record InvestigateClaimInput(UserRole ActorRole, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseTransitionInput(ActorRole, ActorId, Now);

public sealed class InvestigateClaimTransition : ClaimBaseTransition<InvestigateClaimInput>
{
    public InvestigateClaimTransition()
        : base(new ClaimUnderInvestigationState())
    {
        AllowedFrom(new ClaimOpenState())
            .Require(input => new HasPermittedRole(input.ActorRole, UserRole.Handler, UserRole.Supervisor));
    }
}
