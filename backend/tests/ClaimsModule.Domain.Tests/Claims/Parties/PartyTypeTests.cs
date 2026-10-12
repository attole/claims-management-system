using ClaimsModule.Domain.Claims.Parties;

namespace ClaimsModule.Domain.Tests.Claims.Parties;

public sealed class PartyTypeTests
{
    [Fact]
    public void APerson_UsesThePersonName()
    {
        PartyType person = new PersonPartyType();

        person.UsesPersonName.Should().BeTrue();
        person.UsesCompanyName.Should().BeFalse();
        person.Marker.Should().Be(PartyTypeKind.Person);
    }

    [Fact]
    public void ACompany_UsesTheCompanyName()
    {
        PartyType company = new CompanyPartyType();

        company.UsesPersonName.Should().BeFalse();
        company.UsesCompanyName.Should().BeTrue();
        company.Marker.Should().Be(PartyTypeKind.Company);
    }
}
