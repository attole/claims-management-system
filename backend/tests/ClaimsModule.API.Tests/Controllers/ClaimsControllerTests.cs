using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Paging;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class ClaimsControllerTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task List_WithoutAToken_Answers401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/claims");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_WithoutParameters_ReturnsTheFirstPageOfTheDefaultSize()
    {
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/claims");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page.Should().BeEquivalentTo(new { Page = 1, PageSize = 25, TotalCount = 0 });
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_SearchByClaimNumber_ReturnsTheClaimOfTheOrganisation()
    {
        var organisationId = Guid.NewGuid();
        var number = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims?search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Select(claim => claim.ClaimNumber).Should().Equal(number);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_AClaimWithACauseOfLoss_ReturnsItsCode()
    {
        var organisationId = Guid.NewGuid();
        var number = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims?search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Select(claim => claim.CauseOfLossCode).Should().Equal(ApiFactory.SeedCauseOfLossCode);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_AClaimWithoutReserves_ReturnsAZeroReserveTotal()
    {
        var organisationId = Guid.NewGuid();
        var number = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims?search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Select(claim => claim.TotalReserves).Should().Equal(0m);
    }

    [ApiFact]
    [Trait("Rule", "S:UI-List")]
    public async Task List_TwoClaimsOfTheOrganisation_CountsBoth()
    {
        var organisationId = Guid.NewGuid();
        await _factory.SeedClaimAsync(organisationId);
        await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync("/api/claims");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.TotalCount.Should().Be(2);
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task List_ClaimOfAnotherOrganisation_IsNotReturned()
    {
        var number = await _factory.SeedClaimAsync(Guid.NewGuid());
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync($"/api/claims?search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_RepeatedStatusParameters_FilterByEveryStatusGiven()
    {
        var organisationId = Guid.NewGuid();
        var number = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims?status=Open&status=Draft&search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Select(claim => claim.Status).Should().Equal("Draft");
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_StatusTheClaimDoesNotHave_ReturnsNothing()
    {
        var organisationId = Guid.NewGuid();
        var number = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims?status=Closed&search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task List_UnknownStatusName_Answers422InTheErrorBody()
    {
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/claims?status=Bogus");

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body!.Type.Should().Be("ValidationError");
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task List_EndDateBeforeTheStartDate_Answers422()
    {
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/claims?dateFrom=2026-03-01&dateTo=2026-02-01");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task List_PageSizeAboveTheCap_Answers422()
    {
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/claims?pageSize=101");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    private HttpClient CreateClient(Guid organisationId)
    {
        return _factory.CreateClientFor(Guid.NewGuid(), organisationId, "Handler");
    }
}
