using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.ReserveChanges.Inputs;

public sealed record ApproveReserveInput(
    decimal ClaimApprovedTotal,
    decimal ReserveApprovedBalance,
    bool HasAggregateLimitOverride,
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now) : ReserveApprovalInput(ClaimApprovedTotal, ReserveApprovedBalance, ActorId, Now);
