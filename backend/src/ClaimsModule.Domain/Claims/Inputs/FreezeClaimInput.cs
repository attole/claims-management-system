using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record FreezeClaimInput(
    bool HasPendingReserveHistory,
    bool HasUnpostedReserveHistory,
    bool HasOpenReserveBalance,
    PolicyPeriod PolicyPeriod,
    CauseCodes CauseCodes) : ClaimTransitionInput(PolicyPeriod, CauseCodes);
