using FluentValidation;

namespace ClaimsModule.Application.Claims.RiskObjects;

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
