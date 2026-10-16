namespace ClaimsModule.Domain.ReserveChanges.Inputs;

public sealed record AutoApproveReserveInput(
    decimal ClaimApprovedTotal,
    decimal ReserveApprovedBalance,
    Guid ActorId,
    DateTimeOffset Now) : ReserveApprovalInput(ClaimApprovedTotal, ReserveApprovedBalance, ActorId, Now)
{
    public override Guid ReviewerId => Guid.Empty;
}
