namespace ClaimsModule.Domain.ReserveComponents.History.Inputs;

internal sealed record AutoApproveRowInput(
    decimal ClaimApprovedTotal,
    decimal ReserveApprovedBalance,
    Guid ActorId,
    DateTimeOffset Now) : RowApprovalInput(ClaimApprovedTotal, ReserveApprovedBalance, ActorId, Now)
{
    public override Guid ReviewerId => Guid.Empty;
}
