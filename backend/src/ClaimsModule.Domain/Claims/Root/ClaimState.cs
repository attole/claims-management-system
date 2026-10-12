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

public sealed record DraftState() : ClaimState(ClaimStatus.Draft);

public sealed record OpenState() : ClaimState(ClaimStatus.Open);

public sealed record UnderInvestigationState() : ClaimState(ClaimStatus.UnderInvestigation);

public sealed record PendingPaymentState() : ClaimState(ClaimStatus.PendingPayment);

public sealed record ClosedState() : ClaimState(ClaimStatus.Closed);

public sealed record ReopenedState() : ClaimState(ClaimStatus.Reopened);

public sealed record WithdrawnState() : ClaimState(ClaimStatus.Withdrawn);
