using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class ClaimRiskObjectConfigurationTests
{
    [Fact]
    [Trait("Rule", "S:DATA-EfConfig")]
    public void Configure_RiskObject_ReadsBackItsDetails()
    {
        using var database = new TestDatabase();
        var written = TestClaims.Ready(database, Ids.Next());
        TestClaims.Save(database, written);
        using var context = database.CreateContext();

        var result = context.Set<ClaimRiskObject>().Single();

        result.ClaimId.Should().Be(written.Id);
        result.Details.Should().Be(written.RiskObjects[0].Details);
    }

    [Theory]
    [InlineData(AssetType.Vehicle, "Vehicle")]
    [InlineData(AssetType.Property, "Property")]
    [InlineData(AssetType.Person, "Person")]
    [InlineData(AssetType.Equipment, "Equipment")]
    [InlineData(AssetType.Other, "Other")]
    public void Configure_AssetType_IsStoredAsTheEnumMemberName(AssetType assetType, string stored)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var details = context.Model.FindEntityType(typeof(ClaimRiskObject))!
            .FindComplexProperty(nameof(ClaimRiskObject.Details))!.ComplexType;
        var converter = details.FindProperty(nameof(RiskObjectDetails.AssetType))!.GetTypeMapping().Converter!;

        var result = converter.ConvertToProvider(assetType);

        result.Should().Be(stored);
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void Configure_RemovedRiskObject_IsHiddenFromQueries()
    {
        using var database = new TestDatabase();
        var claim = TestClaims.Ready(database, Ids.Next());
        TestClaims.Save(database, claim);
        using var context = database.CreateContext();
        var stored = context.Claims.Include(found => found.RiskObjects).Single();
        stored.RiskObjects[0].Remove();
        context.SaveChanges();

        var result = context.Set<ClaimRiskObject>().ToList();

        result.Should().BeEmpty();
    }
}
