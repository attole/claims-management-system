using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Claims.Dtos;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class ReferenceControllerTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-causes")]
    public async Task ListCauseCodes_SeededOrganisation_ReturnsTheActiveCodes()
    {
        using var client = CreateSeedClient();

        var response = await client.GetAsync("/api/reference/cause-of-loss-codes");

        var codes = await response.Content.ReadFromJsonAsync<List<CauseCodeDto>>();
        codes!.Select(code => code.Code).Should().Contain("COL-FIRE");
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-causes")]
    public async Task ListCauseCodes_WithAPerilCategory_ReturnsOnlyThatCategory()
    {
        using var client = CreateSeedClient();
        var all = await client.GetFromJsonAsync<List<CauseCodeDto>>("/api/reference/cause-of-loss-codes");
        var category = all![0].PerilCategory;

        var response = await client.GetAsync($"/api/reference/cause-of-loss-codes?perilCategory={category}");

        var codes = await response.Content.ReadFromJsonAsync<List<CauseCodeDto>>();
        codes!.Should().NotBeEmpty().And.OnlyContain(code => code.PerilCategory == category);
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task ListCauseCodes_UnknownPerilCategory_Answers422()
    {
        using var client = CreateSeedClient();

        var response = await client.GetAsync("/api/reference/cause-of-loss-codes?perilCategory=Bogus");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task ListCauseCodes_AnotherOrganisation_SeesNoCodes()
    {
        using var client = _factory.CreateClientFor(Guid.NewGuid(), Guid.NewGuid(), "Handler");

        var response = await client.GetAsync("/api/reference/cause-of-loss-codes");

        var codes = await response.Content.ReadFromJsonAsync<List<CauseCodeDto>>();
        codes.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-statuses")]
    public async Task ListClaimStatuses_SeededOrganisation_ReturnsSevenStatuses()
    {
        using var client = CreateSeedClient();

        var response = await client.GetAsync("/api/reference/claim-statuses");

        var statuses = await response.Content.ReadFromJsonAsync<List<ClaimStatusInfoDto>>();
        statuses!.Select(status => status.Status).Should().HaveCount(7);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-statuses")]
    public async Task ListClaimStatuses_ADraftClaim_CanMoveToOpen()
    {
        using var client = CreateSeedClient();

        var response = await client.GetAsync("/api/reference/claim-statuses");

        var statuses = await response.Content.ReadFromJsonAsync<List<ClaimStatusInfoDto>>();
        statuses!.Single(status => status.Status == "Draft").NextStatuses.Should().Contain("Open");
    }

    private HttpClient CreateSeedClient()
    {
        return _factory.CreateClientFor(Guid.NewGuid(), ApiFactory.SeedOrganisationId, "Handler");
    }
}
