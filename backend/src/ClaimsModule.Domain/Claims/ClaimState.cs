using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Claims;

public enum ClaimStateKind
{
    Draft,
    Open,
    UnderInvestigation,
    PendingPayment,
    Closed,
    Reopened,
    Withdrawn,
}

public abstract record ClaimState(ClaimStateKind Marker) : Marked<ClaimStateKind>(Marker)
{
    public virtual bool IsEditable => true;

    public virtual bool IsDraft => false;

    public static ClaimState From(ClaimStateKind kind)
    {
        return ClosedSet<ClaimState, ClaimStateKind>.From(kind);
    }
}

public sealed record ClaimDraftState() : ClaimState(ClaimStateKind.Draft)
{
    public override bool IsDraft => true;
}

public sealed record ClaimOpenState() : ClaimState(ClaimStateKind.Open);

public sealed record ClaimUnderInvestigationState() : ClaimState(ClaimStateKind.UnderInvestigation);

public sealed record ClaimPendingPaymentState() : ClaimState(ClaimStateKind.PendingPayment);

public sealed record ClaimClosedState() : ClaimState(ClaimStateKind.Closed)
{
    public override bool IsEditable => false;
}

public sealed record ClaimReopenedState() : ClaimState(ClaimStateKind.Reopened);

public sealed record ClaimWithdrawnState() : ClaimState(ClaimStateKind.Withdrawn)
{
    public override bool IsEditable => false;
}
