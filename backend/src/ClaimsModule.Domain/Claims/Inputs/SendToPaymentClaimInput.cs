using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record SendToPaymentClaimInput(bool HasApprovedReserve, PolicyPeriod PolicyPeriod, CauseCodes CauseCodes)
    : ClaimTransitionInput(PolicyPeriod, CauseCodes)
{
    public bool IsLiabilityConfirmed { get; init; } = false;
}
