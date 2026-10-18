using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Inputs;

public abstract record ClaimBaseOperationInput(PolicyPeriod PolicyPeriod, CauseCodes CauseCodes);
