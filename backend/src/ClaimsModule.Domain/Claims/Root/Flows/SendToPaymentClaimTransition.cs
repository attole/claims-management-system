using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class SendToPaymentClaimTransition
    : ClaimBaseTransition<SendToPaymentClaimTransition, SendToPaymentClaimInput>
{
    public SendToPaymentClaimTransition() : base(new ClaimPendingPaymentState())
    {
        AllowedFrom(new ClaimOpenState())
            .Require(input => new HasApprovedReserveForOpenClaim(input.Reserves));
        AllowedFrom(new ClaimUnderInvestigationState())
            .Require(input => new HasApprovedReserveForClaimUnderInvestigation(input.Reserves))
            .Require(input => new HasLiabilityConfirmation(input.IsLiabilityConfirmed));
    }
}
