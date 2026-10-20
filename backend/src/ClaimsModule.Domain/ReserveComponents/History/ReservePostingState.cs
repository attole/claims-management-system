using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.ReserveComponents.History;

public enum ReservePostingStateKind
{
    Pending,
    Posted,
    Failed,
    Cancelled,
}

public abstract record ReservePostingState(ReservePostingStateKind Marker) : Marked<ReservePostingStateKind>(Marker)
{
    public virtual bool IsPosted => false;

    public static ReservePostingState From(ReservePostingStateKind kind)
    {
        return ClosedSet<ReservePostingState, ReservePostingStateKind>.From(kind);
    }
}

public sealed record ReservePostingPendingState() : ReservePostingState(ReservePostingStateKind.Pending);

public sealed record ReservePostingPostedState() : ReservePostingState(ReservePostingStateKind.Posted)
{
    public override bool IsPosted => true;
}

public sealed record ReservePostingFailedState() : ReservePostingState(ReservePostingStateKind.Failed);

public sealed record ReservePostingCancelledState() : ReservePostingState(ReservePostingStateKind.Cancelled);
