using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record ReopenClaimInput(
    UserRole ActorRole,
    PolicyPeriod PolicyPeriod,
    CauseCodes CauseCodes,
    DateTimeOffset Now) : ClaimTransitionInput(PolicyPeriod, CauseCodes, Now);
