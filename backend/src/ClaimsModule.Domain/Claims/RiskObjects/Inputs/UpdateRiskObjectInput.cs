using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.RiskObjects.Inputs;

public sealed record UpdateRiskObjectInput(Guid RiskObjectId, RiskObjectDetails Details, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference);
