namespace ClaimsModule.Application.ReserveComponents.Dtos;

public sealed record ReserveComponentDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public Guid ClaimId { get; init; } = Guid.Empty;

    public string ComponentType { get; init; } = string.Empty;

    public string State { get; init; } = string.Empty;

    public decimal ApprovedBalance { get; init; } = 0m;

    public string Notes { get; init; } = string.Empty;
}
