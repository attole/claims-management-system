using ClaimsModule.Application.Tests.Support.Claims;

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
}
