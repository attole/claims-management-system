using AutoMapper;
using ClaimsModule.Application.Claims.Loss;
using ClaimsModule.Application.Tests.Support.Claims;

namespace ClaimsModule.Application.Tests.Claims.Loss;

public sealed class LossMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        Action act = configuration.AssertConfigurationIsValid;

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_LossEvent_FlattensTheDetailsAndFlagsAnUnsetEstimate()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var lossEvent = ApplicationClaims.Draft().LossEvent;

        var result = mapper.Map<LossDetailsDto>(lossEvent);

        result.ClaimId.Should().Be(lossEvent.ClaimId);
        result.HasEstimatedLossAmount.Should().BeFalse();
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(configuration => configuration.AddProfile<LossMappingProfile>());
    }
}
