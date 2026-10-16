using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record SetLimitOverrideInput(
    bool HasAggregateLimitOverride,
    UserRole ActorRole,
    decimal ClaimApprovedTotal,
    LossReference LossReference) : ClaimBaseOperationInput(LossReference);
