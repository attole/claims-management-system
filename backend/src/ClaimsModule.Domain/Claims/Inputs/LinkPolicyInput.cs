using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record LinkPolicyInput(Guid PolicyId, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference);
