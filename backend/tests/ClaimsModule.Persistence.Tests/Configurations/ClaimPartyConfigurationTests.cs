using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class ClaimPartyConfigurationTests
{
    [Fact]
    [Trait("Rule", "S:BR-P-02")]
    public void Configure_Party_ReadsBackItsDetailsAndActiveFlag()
    {
        using var database = new TestDatabase();
        var written = TestClaims.Ready(database, Ids.Next());
        TestClaims.Save(database, written);
        using var context = database.CreateContext();

        var result = context.Set<ClaimParty>().Single();

        result.ClaimId.Should().Be(written.Id);
        result.Details.Should().Be(written.Parties[0].Details);
        result.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(PartyRoleKind.Claimant, "Claimant")]
    [InlineData(PartyRoleKind.Insured, "Insured")]
    [InlineData(PartyRoleKind.ThirdParty, "ThirdParty")]
    [InlineData(PartyRoleKind.Witness, "Witness")]
    [InlineData(PartyRoleKind.Attorney, "Attorney")]
    public void Configure_Role_IsStoredAsTheEnumMemberName(PartyRoleKind kind, string stored)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var details = context.Model.FindEntityType(typeof(ClaimParty))!
            .FindComplexProperty(nameof(ClaimParty.Details))!.ComplexType;
        var converter = details.FindProperty(nameof(PartyDetails.Role))!.GetValueConverter()!;

        var result = converter.ConvertToProvider(PartyRole.From(kind));

        result.Should().Be(stored);
    }

    [Theory]
    [InlineData(PartyTypeKind.Person, "Person")]
    [InlineData(PartyTypeKind.Company, "Company")]
    public void Configure_Type_IsStoredAsTheEnumMemberName(PartyTypeKind kind, string stored)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var details = context.Model.FindEntityType(typeof(ClaimParty))!
            .FindComplexProperty(nameof(ClaimParty.Details))!.ComplexType;
        var converter = details.FindProperty(nameof(PartyDetails.Type))!.GetValueConverter()!;

        var result = converter.ConvertToProvider(PartyType.From(kind));

        result.Should().Be(stored);
    }
}
