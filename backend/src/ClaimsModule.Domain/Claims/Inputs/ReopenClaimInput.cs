using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record ReopenClaimInput(UserRole ActorRole, PolicyPeriod PolicyPeriod, CauseCodes CauseCodes)
    : ClaimTransitionInput(PolicyPeriod, CauseCodes);
