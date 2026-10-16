using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record SetSeverityInput(ClaimSeverity Severity, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference);
