using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Loss.Inputs;

public sealed record UpdateLossDetailsInput(LossDetails Loss, PolicyPeriod PolicyPeriod, CauseCodes CauseCodes)
    : ClaimBaseOperationInput(PolicyPeriod, CauseCodes);
