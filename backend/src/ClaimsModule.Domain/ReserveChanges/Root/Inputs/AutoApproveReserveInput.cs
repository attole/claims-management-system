using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.ReserveChanges.Root.Inputs;

public sealed record AutoApproveReserveInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ReserveSnapshot Snapshot) : ReserveChangeBaseTransitionInput(ActorRole, ActorId, Now, Snapshot);
