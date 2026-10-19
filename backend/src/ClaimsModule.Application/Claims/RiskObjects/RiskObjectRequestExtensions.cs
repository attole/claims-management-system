using ClaimsModule.Domain.Claims.RiskObjects;

namespace ClaimsModule.Application.Claims.RiskObjects;

public static class RiskObjectRequestExtensions
{
    public static RiskObjectDetails ToDetails(this RiskObjectRequest request)
    {
        return new RiskObjectDetails(request.AssetType, request.AssetDescription)
        {
            DamageDescription = request.DamageDescription ?? string.Empty,
            IsPrimary = request.IsPrimary,
            AssetReference = request.AssetReference ?? string.Empty,
        };
    }
}
