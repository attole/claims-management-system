using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Rules;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class OpenClaimTransition : ClaimBaseTransition<OpenClaimTransition, ClaimTransitionInput>
{
    public OpenClaimTransition() : base(new ClaimOpenState())
    {
        AllowedFrom(new ClaimDraftState())
            .Require(new HasNoCriticalIssuesForOpen())
            .Require(new HasNoUnacknowledgedPolicyWarning())
            .Require(new HasClaimant())
            .Require(new HasAssignedHandler());
        AllowedFrom(new ClaimUnderInvestigationState());
        AllowedFrom(new ClaimReopenedState());
    }
}
