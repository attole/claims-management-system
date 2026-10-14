using ClaimsModule.Domain.Claims.RiskObjects;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects;

public sealed class AssetTypeTests
{
    [Fact]
    [Trait("Rule", "S:RISK-AssetTypes")]
    public void AssetTypes_AreTheFiveDefinedOnes()
    {
        Enum.GetValues<AssetType>().Should().BeEquivalentTo(
            [AssetType.Vehicle, AssetType.Property, AssetType.Person, AssetType.Equipment, AssetType.Other]);
    }
}
