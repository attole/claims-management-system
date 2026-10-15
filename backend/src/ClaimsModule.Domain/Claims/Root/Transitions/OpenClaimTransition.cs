using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Transitions;

public sealed record OpenClaimInput(UserRole ActorRole, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseTransitionInput(ActorRole, ActorId, Now);

public sealed class OpenClaimTransition : ClaimBaseTransition<OpenClaimInput>
{
    public OpenClaimTransition()
        : base(new ClaimOpenState())
    {
        AllowedFrom(new ClaimDraftState())
            .Require(new HasNoBlockingIssues())
            .Require(new HasNoUnacknowledgedPolicyWarning())
            .Require(new HasActiveClaimant());
        AllowedFrom(new ClaimUnderInvestigationState())
            .Require(input => new HasPermittedRole(input.ActorRole, UserRole.Handler));
        AllowedFrom(new ClaimReopenedState());
    }
}
