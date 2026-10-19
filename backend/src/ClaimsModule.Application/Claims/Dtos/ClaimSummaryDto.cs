namespace ClaimsModule.Application.Claims.Dtos;

public sealed record ClaimSummaryDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public string ClaimNumber { get; init; } = string.Empty;

    public string ClientName { get; init; } = string.Empty;

    public string PolicyNumber { get; init; } = string.Empty;

    public DateTimeOffset LossDate { get; init; } = DateTimeOffset.MinValue;

    public string Status { get; init; } = string.Empty;

    public decimal TotalReserves { get; init; } = 0m;
}
