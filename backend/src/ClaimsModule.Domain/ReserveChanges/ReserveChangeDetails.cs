using ClaimsModule.Domain.Reserves;

namespace ClaimsModule.Domain.ReserveChanges;

public sealed record ReserveChangeDetails(
    Guid ReserveId,
    Guid ClaimId,
    ReserveComponentType ComponentType,
    ReserveTransactionType TransactionType,
    decimal Amount,
    string ChangeReason,
    int ChangeSequence)
{
    private const decimal AutoApprovalLimit = 10_000m;

    public decimal AbsoluteAmount => Math.Abs(Amount);

    public bool IsWithinAutoApprovalLimit => AbsoluteAmount <= AutoApprovalLimit;

    public decimal BalanceAfter(decimal approvedBalance)
    {
        decimal balanceAfter = approvedBalance + Amount;

        return balanceAfter;
    }

    public bool CanHoldBalanceAfter(decimal approvedBalance)
    {
        bool canHold = ComponentType.CanHoldBalance(BalanceAfter(approvedBalance));

        return canHold;
    }
}
