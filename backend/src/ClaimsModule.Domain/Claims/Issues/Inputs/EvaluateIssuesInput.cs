using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Issues.Inputs;

public sealed record EvaluateIssuesInput(PolicyPeriod PolicyPeriod, CauseCodes CauseCodes, DateTimeOffset Now)
    : ClaimBaseOperationInput(PolicyPeriod, CauseCodes, Now);
