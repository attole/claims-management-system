using AutoMapper;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.ReserveComponents;

namespace ClaimsModule.Application.Tests.ReserveComponents.History.Dtos;

public sealed class ReserveHistoryMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        Action act = configuration.AssertConfigurationIsValid;

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_History_FlattensTheDetailsAndTheStates()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 5000m, ApplicationReserves.Submitter);

        var result = mapper.Map<ReserveHistoryDto>(history);

        result.ClaimId.Should().Be(claim.Id);
        result.ComponentType.Should().Be("Indemnity");
        result.TransactionType.Should().Be("Add");
        result.Amount.Should().Be(5000m);
        result.ApprovalStatus.Should().Be("AutoApproved");
        result.PostingStatus.Should().Be("Pending");
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Map_History_WithApprovedBalances_GivesThePreviousAndTheNewBalance()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 5000m, ApplicationReserves.Submitter);

        var result = mapper.Map<ReserveHistoryDto>(history);

        result.HasBalances.Should().BeTrue();
        result.PreviousBalance.Should().Be(0m);
        result.NewBalance.Should().Be(5000m);
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Map_History_BeforeAnApproval_HasNoBalances()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 5_000_000m, ApplicationReserves.Submitter);

        var result = mapper.Map<ReserveHistoryDto>(history);

        result.HasBalances.Should().BeFalse();
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(configuration => configuration.AddProfile<ReserveHistoryMappingProfile>());
    }
}
