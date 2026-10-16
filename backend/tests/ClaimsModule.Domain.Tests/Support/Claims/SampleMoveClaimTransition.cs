using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;

namespace ClaimsModule.Domain.Tests.Support.Claims;

internal sealed class SampleMoveClaimTransition<TTarget>
    : ClaimBaseTransition<SampleMoveClaimTransition<TTarget>, ClaimTransitionInput>
    where TTarget : ClaimState, new()
{
    public SampleMoveClaimTransition() : base(new TTarget())
    {
        AllowedFrom(new ClaimDraftState());
    }
}
