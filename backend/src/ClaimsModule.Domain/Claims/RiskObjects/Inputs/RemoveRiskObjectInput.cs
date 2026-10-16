using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Claims.RiskObjects.Inputs;

public sealed record RemoveRiskObjectInput(Guid RiskObjectId, DateTimeOffset Now, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference)
{
    public DateTimeOffset Now { get; } = InputTime.Required(Now);
}
