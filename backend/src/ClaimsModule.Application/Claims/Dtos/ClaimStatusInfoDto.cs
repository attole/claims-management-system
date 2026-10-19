namespace ClaimsModule.Application.Claims.Dtos;

public sealed record ClaimStatusInfoDto(string Status, IReadOnlyList<string> NextStatuses);
