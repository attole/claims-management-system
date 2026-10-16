using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;

namespace ClaimsModule.Domain.Tests.Support.Claims;

internal sealed class FailingSampleClaimTransition
    : ClaimBaseTransition<FailingSampleClaimTransition, ClaimTransitionInput>
{
    public FailingSampleClaimTransition() : base(new ClaimOpenState())
    {
        AllowedFrom(new ClaimDraftState())
            .Require(new CriticalSampleClaimRule());
    }
}
