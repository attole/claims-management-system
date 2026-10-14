using ClaimsModule.Domain.Claims.Parties;

namespace ClaimsModule.Domain.Tests.Claims.Parties;

public sealed class PartyTypeTests
{
    [Fact]
    public void APerson_UsesThePersonName()
    {
        PartyType person = new PartyPersonType();

        person.HasPersonName.Should().BeTrue();
        person.HasCompanyName.Should().BeFalse();
        person.Marker.Should().Be(PartyTypeKind.Person);
    }

    [Fact]
    public void ACompany_UsesTheCompanyName()
    {
        PartyType company = new PartyCompanyType();

        company.HasPersonName.Should().BeFalse();
        company.HasCompanyName.Should().BeTrue();
        company.Marker.Should().Be(PartyTypeKind.Company);
    }
}
