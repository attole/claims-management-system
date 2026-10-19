using ClaimsModule.Domain.Claims.RiskObjects;
using FluentValidation;

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

public sealed class RiskObjectRequestValidator : AbstractValidator<RiskObjectRequest>
{
    public RiskObjectRequestValidator()
    {
        RuleFor(request => request.AssetType).IsInEnum();
        RuleFor(request => request.AssetDescription).NotNull();
        RuleFor(request => request.AssetDescription).MaximumLength(ClaimFieldLengths.AssetDescription);
        RuleFor(request => request.AssetReference).MaximumLength(ClaimFieldLengths.AssetReference);
    }
}
