using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Issues.Inputs;

public sealed record EvaluateIssuesInput(PolicyPeriod PolicyPeriod, CauseCodes CauseCodes)
    : ClaimBaseOperationInput(PolicyPeriod, CauseCodes);
