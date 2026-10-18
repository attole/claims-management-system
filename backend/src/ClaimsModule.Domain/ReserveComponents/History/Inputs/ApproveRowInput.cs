using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.ReserveComponents.History.Inputs;

internal sealed record ApproveRowInput(
    decimal ClaimApprovedTotal,
    decimal ReserveApprovedBalance,
    bool HasAggregateLimitOverride,
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now) : RowApprovalInput(ClaimApprovedTotal, ReserveApprovedBalance, ActorId, Now);
