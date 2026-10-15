using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class OpenClaimTransition : ClaimBaseTransition<OpenClaimTransition, OpenClaimInput>
{
    public OpenClaimTransition() : base(new ClaimOpenState())
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
