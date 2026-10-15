using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;

namespace ClaimsModule.Domain.Tests.Support.Claims;

internal sealed class SampleClaimTransition : ClaimBaseTransition<SampleClaimTransitionInput>
{
    public SampleClaimTransition() : base(new ClaimOpenState())
    {
        AllowedFrom(new ClaimDraftState());
    }
}
