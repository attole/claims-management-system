using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record SetLimitOverrideInput(
    bool HasAggregateLimitOverride,
    UserRole ActorRole,
    decimal ClaimApprovedTotal,
    PolicyPeriod PolicyPeriod,
    CauseCodes CauseCodes) : ClaimBaseOperationInput(PolicyPeriod, CauseCodes);
