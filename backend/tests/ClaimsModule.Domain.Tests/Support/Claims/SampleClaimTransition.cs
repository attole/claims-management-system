using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;

namespace ClaimsModule.Domain.Tests.Support.Claims;

internal sealed class SampleClaimTransition : ClaimBaseTransition<SampleClaimTransition, ClaimTransitionInput>
{
    public SampleClaimTransition() : base(new ClaimOpenState())
    {
        AllowedFrom(new ClaimDraftState());
    }
}
