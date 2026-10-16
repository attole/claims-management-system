using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record SendToPaymentClaimInput(bool HasApprovedReserve, LossReference LossReference)
    : ClaimTransitionInput(LossReference)
{
    public bool IsLiabilityConfirmed { get; init; } = false;
}
