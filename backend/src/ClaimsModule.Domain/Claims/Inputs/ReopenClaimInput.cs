using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record ReopenClaimInput(UserRole ActorRole, LossReference LossReference)
    : ClaimTransitionInput(LossReference);
