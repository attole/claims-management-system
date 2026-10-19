namespace ClaimsModule.Application.ReserveComponents;

public sealed record ClaimReserveFacts(
    bool HasPendingHistory,
    bool HasUnpostedHistory,
    bool HasOpenBalance,
    bool HasApprovedReserve,
    decimal ApprovedTotal);
