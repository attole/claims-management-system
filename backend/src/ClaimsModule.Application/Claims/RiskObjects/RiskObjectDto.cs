namespace ClaimsModule.Application.Claims.RiskObjects;

public sealed record RiskObjectDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public Guid ClaimId { get; init; } = Guid.Empty;

    public string AssetType { get; init; } = string.Empty;

    public string AssetDescription { get; init; } = string.Empty;

    public string DamageDescription { get; init; } = string.Empty;

    public bool IsPrimary { get; init; } = false;

    public string AssetReference { get; init; } = string.Empty;

    public bool IsDeleted { get; init; } = false;
}
