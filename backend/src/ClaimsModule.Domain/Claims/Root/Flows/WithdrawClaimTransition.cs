using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.StateChanges.Rules;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class WithdrawClaimTransition : ClaimBaseTransition<WithdrawClaimTransition, WithdrawClaimInput>
{
    public WithdrawClaimTransition() : base(new ClaimWithdrawnState())
    {
        AllowedFrom(new ClaimOpenState(), new ClaimUnderInvestigationState())
            .Require(input => new HasStateChangeReason(input.Reason));
    }
}
