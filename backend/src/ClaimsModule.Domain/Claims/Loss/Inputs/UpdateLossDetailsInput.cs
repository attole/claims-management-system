using ClaimsModule.Domain.Claims.Inputs;

namespace ClaimsModule.Domain.Claims.Loss.Inputs;

public sealed record UpdateLossDetailsInput(LossDetails Loss, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference);
