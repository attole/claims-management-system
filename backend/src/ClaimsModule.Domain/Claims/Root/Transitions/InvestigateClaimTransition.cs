using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Transitions;

public sealed record InvestigateClaimInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseTransitionInput(ActorRole, ActorId, Now, Reference);

internal sealed class InvestigateClaimTransition : ClaimBaseTransition<InvestigateClaimInput>
{
    public InvestigateClaimTransition() : base(new ClaimUnderInvestigationState())
    {
        AllowedFrom(new ClaimOpenState())
            .Require(input => new HasPermittedRole(input.ActorRole, UserRole.Handler, UserRole.Supervisor));
    }
}
