using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Claims.Root;

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

public abstract record ClaimState(ClaimStateKind Marker) : Marked<ClaimStateKind>(Marker);

public sealed record ClaimDraftState() : ClaimState(ClaimStateKind.Draft);

public sealed record ClaimOpenState() : ClaimState(ClaimStateKind.Open);

public sealed record ClaimUnderInvestigationState() : ClaimState(ClaimStateKind.UnderInvestigation);

public sealed record ClaimPendingPaymentState() : ClaimState(ClaimStateKind.PendingPayment);

public sealed record ClaimClosedState() : ClaimState(ClaimStateKind.Closed);

public sealed record ClaimReopenedState() : ClaimState(ClaimStateKind.Reopened);

public sealed record ClaimWithdrawnState() : ClaimState(ClaimStateKind.Withdrawn);
