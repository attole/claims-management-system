using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record SendToPaymentClaimInput(
    bool HasApprovedReserve,
    PolicyPeriod PolicyPeriod,
    CauseCodes CauseCodes,
    DateTimeOffset Now) : ClaimTransitionInput(PolicyPeriod, CauseCodes, Now)
{
    public bool IsLiabilityConfirmed { get; init; } = false;
}
