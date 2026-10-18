using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.RiskObjects.Inputs;

public sealed record RemoveRiskObjectInput(
    Guid RiskObjectId,
    DateTimeOffset Now,
    PolicyPeriod PolicyPeriod,
    CauseCodes CauseCodes) : ClaimBaseOperationInput(PolicyPeriod, CauseCodes)
{
    public DateTimeOffset Now { get; } = InputTime.Required(Now);
}
