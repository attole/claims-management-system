namespace ClaimsModule.Application.ReserveComponents.History;

public sealed record ReserveHistoryStatusDto(Guid ReserveHistoryId, string PreviousStatus, string NewStatus);
