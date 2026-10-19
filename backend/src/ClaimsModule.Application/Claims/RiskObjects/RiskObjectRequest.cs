using ClaimsModule.Domain.Claims.RiskObjects;

namespace ClaimsModule.Application.Claims.RiskObjects;

public sealed record RiskObjectRequest(AssetType AssetType, string AssetDescription)
{
    public string DamageDescription { get; init; } = string.Empty;

    public bool IsPrimary { get; init; } = false;

    public string AssetReference { get; init; } = string.Empty;

    public RiskObjectDetails ToDetails()
    {
        return new RiskObjectDetails(AssetType, AssetDescription)
        {
            DamageDescription = DamageDescription ?? string.Empty,
            IsPrimary = IsPrimary,
            AssetReference = AssetReference ?? string.Empty,
        };
    }
}
