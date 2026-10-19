namespace ClaimsModule.Application.Claims.StateChanges;

public sealed record ClaimStatusDto(Guid ClaimId, string PreviousStatus, string NewStatus);
