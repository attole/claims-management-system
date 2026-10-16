namespace ClaimsModule.Domain.ReserveChanges.Inputs;

public abstract record ReserveApprovalInput(
    decimal ClaimApprovedTotal,
    decimal ReserveApprovedBalance,
    Guid ActorId,
    DateTimeOffset Now) : ReserveDecisionInput(ActorId, Now);
