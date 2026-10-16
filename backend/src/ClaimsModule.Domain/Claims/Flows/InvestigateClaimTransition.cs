using ClaimsModule.Domain.Claims.Inputs;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class InvestigateClaimTransition
    : ClaimBaseTransition<InvestigateClaimTransition, ClaimTransitionInput>
{
    public InvestigateClaimTransition() : base(new ClaimUnderInvestigationState())
    {
        AllowedFrom(new ClaimOpenState());
    }
}
