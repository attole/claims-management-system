namespace ClaimsModule.Domain.ReserveComponents.History.Inputs;

internal abstract record RowApprovalInput(
    decimal ClaimApprovedTotal,
    decimal ReserveApprovedBalance,
    Guid ActorId,
    DateTimeOffset Now) : RowDecisionInput(ActorId, Now);
