using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Claims.RiskObjects;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.RiskObjects;

public sealed class RiskObjectRequestTests
{
    [Fact]
    public void ToDetails_MapsTheAssetAndThePrimaryFlag()
    {
        var request = ApplicationRequests.Kitchen() with { IsPrimary = true };

        var result = request.ToDetails();

        result.AssetDescription.Should().Be("Kitchen");
        result.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void ToDetails_WithNullOptionalText_UsesEmptyText()
    {
        var request = ApplicationRequests.Kitchen() with
        {
            DamageDescription = null!,
            AssetReference = null!,
        };

        var result = request.ToDetails();

        result.DamageDescription.Should().BeEmpty();
        result.AssetReference.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ForAKnownAssetType_HasNoErrors()
    {
        var validator = new RiskObjectRequestValidator();
        var request = ApplicationRequests.Kitchen();

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnUnknownAssetType_FailsOnTheAssetType()
    {
        var validator = new RiskObjectRequestValidator();
        var request = ApplicationRequests.Kitchen() with { AssetType = (AssetType)99 };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(riskObject => riskObject.AssetType);
    }

    [Fact]
    public void Validate_WithoutADescription_FailsOnTheDescription()
    {
        var validator = new RiskObjectRequestValidator();
        var request = ApplicationRequests.Kitchen() with { AssetDescription = null! };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(riskObject => riskObject.AssetDescription);
    }

    [Fact]
    public void Validate_WithATooLongAssetDescription_FailsOnTheField()
    {
        var validator = new RiskObjectRequestValidator();
        var request = ApplicationRequests.Kitchen() with
        {
            AssetDescription = new string('x', ClaimFieldLengths.AssetDescription + 1),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(riskObject => riskObject.AssetDescription);
    }

    [Fact]
    public void Validate_WithATooLongAssetReference_FailsOnTheField()
    {
        var validator = new RiskObjectRequestValidator();
        var request = ApplicationRequests.Kitchen() with
        {
            AssetReference = new string('x', ClaimFieldLengths.AssetReference + 1),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(riskObject => riskObject.AssetReference);
    }
}
