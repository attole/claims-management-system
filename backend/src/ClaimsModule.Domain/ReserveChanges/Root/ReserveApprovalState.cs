using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.ReserveChanges.Root;

public enum ReserveApprovalStateKind
{
    AutoApproved,
    PendingApproval,
    Approved,
    Rejected,
    Cancelled,
}

public abstract record ReserveApprovalState(ReserveApprovalStateKind Marker) : Marked<ReserveApprovalStateKind>(Marker)
{
    public virtual bool IsApproved => false;
}

public sealed record ReserveAutoApprovedState() : ReserveApprovalState(ReserveApprovalStateKind.AutoApproved)
{
    public override bool IsApproved => true;
}

public sealed record ReservePendingApprovalState() : ReserveApprovalState(ReserveApprovalStateKind.PendingApproval);

public sealed record ReserveApprovedState() : ReserveApprovalState(ReserveApprovalStateKind.Approved)
{
    public override bool IsApproved => true;
}

public sealed record ReserveRejectedState() : ReserveApprovalState(ReserveApprovalStateKind.Rejected);

public sealed record ReserveCancelledState() : ReserveApprovalState(ReserveApprovalStateKind.Cancelled);
