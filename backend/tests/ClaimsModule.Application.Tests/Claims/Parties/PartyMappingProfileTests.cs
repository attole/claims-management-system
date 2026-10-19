using AutoMapper;
using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Tests.Support.Claims;

namespace ClaimsModule.Application.Tests.Claims.Parties;

public sealed class PartyMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        Action act = configuration.AssertConfigurationIsValid;

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_Party_FlattensTheDetails()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var party = ApplicationClaims.WithParties().Parties[0];

        var result = mapper.Map<PartyDto>(party);

        result.Role.Should().Be("Claimant");
        result.FirstName.Should().Be("Ada");
        result.IsActive.Should().BeTrue();
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(configuration => configuration.AddProfile<PartyMappingProfile>());
    }
}
