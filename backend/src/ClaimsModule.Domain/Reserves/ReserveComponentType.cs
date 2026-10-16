using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Reserves;

public enum ReserveComponentTypeKind
{
    Indemnity,
    Expense,
    Alae,
    SubrogationRecoverable,
}

public abstract record ReserveComponentType(ReserveComponentTypeKind Marker)
    : Marked<ReserveComponentTypeKind>(Marker)
{
    protected virtual bool CanGoNegative => false;

    public bool CanHoldBalance(decimal balance)
    {
        bool canHold = CanGoNegative || balance >= 0m;

        return canHold;
    }
}

public sealed record ReserveIndemnityComponentType() : ReserveComponentType(ReserveComponentTypeKind.Indemnity);

public sealed record ReserveExpenseComponentType() : ReserveComponentType(ReserveComponentTypeKind.Expense);

public sealed record ReserveAlaeComponentType() : ReserveComponentType(ReserveComponentTypeKind.Alae);

public sealed record ReserveSubrogationRecoverableComponentType()
    : ReserveComponentType(ReserveComponentTypeKind.SubrogationRecoverable)
{
    protected override bool CanGoNegative => true;
}
