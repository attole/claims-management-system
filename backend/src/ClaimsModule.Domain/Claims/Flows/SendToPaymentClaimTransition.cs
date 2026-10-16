using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class SendToPaymentClaimTransition
    : ClaimBaseTransition<SendToPaymentClaimTransition, SendToPaymentClaimInput>
{
    public SendToPaymentClaimTransition() : base(new ClaimPendingPaymentState())
    {
        AllowedFrom(new ClaimOpenState())
            .Require(input => new HasApprovedReserveForOpenClaim(input.HasApprovedReserve));
        AllowedFrom(new ClaimUnderInvestigationState())
            .Require(input => new HasApprovedReserveForClaimUnderInvestigation(input.HasApprovedReserve))
            .Require(input => new HasLiabilityConfirmation(input.IsLiabilityConfirmed));
    }
}
