namespace ClaimsModule.Domain.ReserveChanges.Root;

public sealed record ReserveChangeDetails(
    Guid ReserveId,
    Guid ClaimId,
    ReserveTransactionType TransactionType,
    decimal Amount,
    string ChangeReason,
    int ChangeSequence);
