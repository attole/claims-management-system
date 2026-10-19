using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Inputs;

public abstract record ClaimBaseOperationInput(PolicyPeriod PolicyPeriod, CauseCodes CauseCodes);
