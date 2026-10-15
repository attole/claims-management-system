namespace ClaimsModule.Domain.Claims.Root;

public sealed record ClaimReserveSnapshot(
    int ApprovedReserveCount,
    bool HasPendingReserveChange,
    bool HasOpenReserveBalance);
