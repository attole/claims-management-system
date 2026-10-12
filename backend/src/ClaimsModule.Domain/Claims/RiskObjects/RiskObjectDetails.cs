namespace ClaimsModule.Domain.Claims.RiskObjects;

public sealed record RiskObjectDetails(AssetType AssetType, string AssetDescription)
{
    public string DamageDescription { get; init; } = string.Empty;

    public bool IsPrimary { get; init; } = false;

    public string AssetReference { get; init; } = string.Empty;
}
