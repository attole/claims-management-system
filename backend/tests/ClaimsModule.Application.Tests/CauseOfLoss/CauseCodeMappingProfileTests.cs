using AutoMapper;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Application.Tests.CauseOfLoss;

public sealed class CauseCodeMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        Action act = configuration.AssertConfigurationIsValid;

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_CauseCode_GivesTheCodeNameAndPerilName()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var code = new CauseOfLossCode(Ids.Next(), Ids.Next(), "HAIL", "Hail damage", PerilCategory.Weather, true);

        var result = mapper.Map<CauseCodeDto>(code);

        result.Should().Be(new CauseCodeDto { Code = "HAIL", Name = "Hail damage", PerilCategory = "Weather" });
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(configuration => configuration.AddProfile<CauseCodeMappingProfile>());
    }
}
