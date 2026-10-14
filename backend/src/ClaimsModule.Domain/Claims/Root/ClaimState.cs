using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Claims.Root;

public enum ClaimStatus
{
    Draft,
    Open,
    UnderInvestigation,
    PendingPayment,
    Closed,
    Reopened,
    Withdrawn,
}

public abstract record ClaimState(ClaimStatus Marker) : Marked<ClaimStatus>(Marker);

public sealed record ClaimDraftState() : ClaimState(ClaimStatus.Draft);

public sealed record ClaimOpenState() : ClaimState(ClaimStatus.Open);

public sealed record ClaimUnderInvestigationState() : ClaimState(ClaimStatus.UnderInvestigation);

public sealed record ClaimPendingPaymentState() : ClaimState(ClaimStatus.PendingPayment);

public sealed record ClaimClosedState() : ClaimState(ClaimStatus.Closed);

public sealed record ClaimReopenedState() : ClaimState(ClaimStatus.Reopened);

public sealed record ClaimWithdrawnState() : ClaimState(ClaimStatus.Withdrawn);
