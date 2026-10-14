using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public sealed class BlockedSampleClaimTransition : ClaimBaseTransition<SampleClaimTransitionInput>
{
    public BlockedSampleClaimTransition()
        : base(new ClaimOpenState())
    {
        AllowedFrom(new ClaimDraftState())
            .Require(new FailingClaimRule());
    }
}
