using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.Issues.Inputs;

public sealed record EvaluateIssuesInput(LossReference LossReference) : ClaimBaseOperationInput(LossReference);
