using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.ReserveComponents.History.Inputs;

internal sealed record RejectRowInput(UserRole ActorRole, Guid ActorId, DateTimeOffset Now)
    : RowDecisionInput(ActorId, Now);
