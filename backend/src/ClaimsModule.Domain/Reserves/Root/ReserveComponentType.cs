using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Reserves.Root;

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
    public virtual bool CanGoNegative => false;
}

public sealed record ReserveIndemnityComponentType() : ReserveComponentType(ReserveComponentTypeKind.Indemnity);

public sealed record ReserveExpenseComponentType() : ReserveComponentType(ReserveComponentTypeKind.Expense);

public sealed record ReserveAlaeComponentType() : ReserveComponentType(ReserveComponentTypeKind.Alae);

public sealed record ReserveSubrogationRecoverableComponentType()
    : ReserveComponentType(ReserveComponentTypeKind.SubrogationRecoverable)
{
    public override bool CanGoNegative => true;
}
