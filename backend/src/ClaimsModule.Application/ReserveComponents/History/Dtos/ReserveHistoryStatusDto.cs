namespace ClaimsModule.Application.ReserveComponents.History.Dtos;

public sealed record ReserveHistoryStatusDto(Guid ReserveHistoryId, string PreviousStatus, string NewStatus);
