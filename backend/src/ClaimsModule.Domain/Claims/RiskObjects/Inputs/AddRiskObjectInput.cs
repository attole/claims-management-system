using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.RiskObjects.Inputs;

public sealed record AddRiskObjectInput(
    RiskObjectDetails Details,
    PolicyPeriod PolicyPeriod,
    CauseCodes CauseCodes,
    DateTimeOffset Now) : ClaimBaseOperationInput(PolicyPeriod, CauseCodes, Now);
