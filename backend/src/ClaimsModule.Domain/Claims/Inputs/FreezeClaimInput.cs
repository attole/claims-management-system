using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record FreezeClaimInput(
    bool HasPendingReserveChange,
    bool HasUnpostedReserveChange,
    bool HasOpenReserveBalance,
    LossReference LossReference) : ClaimTransitionInput(LossReference);
