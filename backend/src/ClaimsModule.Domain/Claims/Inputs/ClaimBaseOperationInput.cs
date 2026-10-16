using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.Inputs;

public abstract record ClaimBaseOperationInput(LossReference LossReference);
