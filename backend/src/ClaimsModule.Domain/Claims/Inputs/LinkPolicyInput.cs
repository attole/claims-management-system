using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record LinkPolicyInput(Guid PolicyId, PolicyPeriod PolicyPeriod, CauseCodes CauseCodes)
    : ClaimBaseOperationInput(PolicyPeriod, CauseCodes);
