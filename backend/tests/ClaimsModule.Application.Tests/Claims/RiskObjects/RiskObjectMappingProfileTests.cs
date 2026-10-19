using AutoMapper;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Application.Tests.Support.Claims;

namespace ClaimsModule.Application.Tests.Claims.RiskObjects;

public sealed class RiskObjectMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        Action act = configuration.AssertConfigurationIsValid;

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_RiskObject_FlattensTheDetails()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var riskObject = ApplicationClaims.WithRiskObjects().RiskObjects[0];

        var result = mapper.Map<RiskObjectDto>(riskObject);

        result.AssetType.Should().Be("Property");
        result.AssetDescription.Should().Be("Kitchen");
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(configuration => configuration.AddProfile<RiskObjectMappingProfile>());
    }
}
