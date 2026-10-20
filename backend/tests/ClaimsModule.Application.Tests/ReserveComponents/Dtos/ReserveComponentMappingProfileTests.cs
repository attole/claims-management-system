using AutoMapper;
using ClaimsModule.Application.ReserveComponents.Dtos;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.ReserveComponents;

namespace ClaimsModule.Application.Tests.ReserveComponents.Dtos;

public sealed class ReserveComponentMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        Action act = configuration.AssertConfigurationIsValid;

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_Component_GivesTheTypeAndTheStateAsNames()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);

        var result = mapper.Map<ReserveComponentDto>(component);

        result.Id.Should().Be(component.Id);
        result.ClaimId.Should().Be(claim.Id);
        result.ComponentType.Should().Be("Indemnity");
        result.State.Should().Be("Active");
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Map_Component_GivesTheApprovedBalance()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        ApplicationReserves.Submit(component, claim, 5000m, ApplicationReserves.Submitter);

        var result = mapper.Map<ReserveComponentDto>(component);

        result.ApprovedBalance.Should().Be(5000m);
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(configuration => configuration.AddProfile<ReserveComponentMappingProfile>());
    }
}
