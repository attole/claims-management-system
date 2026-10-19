using AutoMapper;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Tests.Support.Claims;

namespace ClaimsModule.Application.Tests.Claims.Dtos;

public sealed class ClaimMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        Action act = configuration.AssertConfigurationIsValid;

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_Claim_FlattensTheNumberAndTheState()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var claim = ApplicationClaims.Draft();

        var result = mapper.Map<ClaimDto>(claim);

        result.Id.Should().Be(claim.Id);
        result.ClaimNumber.Should().Be("CLM-2026-0000001");
        result.State.Should().Be("Draft");
        result.Severity.Should().Be("Unspecified");
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(configuration => configuration.AddProfile<ClaimMappingProfile>());
    }
}
