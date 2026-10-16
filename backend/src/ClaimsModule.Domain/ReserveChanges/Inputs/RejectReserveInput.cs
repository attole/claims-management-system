using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.ReserveChanges.Inputs;

public sealed record RejectReserveInput(UserRole ActorRole, Guid ActorId, DateTimeOffset Now)
    : ReserveDecisionInput(ActorId, Now);
