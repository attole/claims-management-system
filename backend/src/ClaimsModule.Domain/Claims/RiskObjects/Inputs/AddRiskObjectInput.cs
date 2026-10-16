using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.RiskObjects.Inputs;

public sealed record AddRiskObjectInput(RiskObjectDetails Details, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference);
