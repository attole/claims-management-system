using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record AssignHandlerInput(Guid HandlerId, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference);
