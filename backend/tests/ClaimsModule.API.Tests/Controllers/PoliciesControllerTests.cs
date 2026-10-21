using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Policies;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class PoliciesControllerTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task Search_WithoutAToken_Answers401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/policies/search");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-policies-search")]
    public async Task Search_WithoutAText_ReturnsTheSeededPoliciesOnTheFirstPage()
    {
        using var client = CreateSeedClient();

        var response = await client.GetAsync("/api/policies/search");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<PolicyDto>>();
        page!.Items.Should().NotBeEmpty();
        page.Page.Should().Be(1);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-policies-search")]
    public async Task Search_ByAPartOfAPolicyNumber_ReturnsThatPolicy()
    {
        using var client = CreateSeedClient();
        var all = await client.GetFromJsonAsync<PagedResult<PolicyDto>>("/api/policies/search");
        var policy = all!.Items[0];
        var part = policy.PolicyNumber[..4];

        var response = await client.GetAsync($"/api/policies/search?q={part}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<PolicyDto>>();
        page!.Items.Select(item => item.Id).Should().Contain(policy.Id);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-policies-search")]
    public async Task Search_ATextNothingMatches_ReturnsAnEmptyPage()
    {
        using var client = CreateSeedClient();

        var response = await client.GetAsync("/api/policies/search?q=no-such-policy-zzzz");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<PolicyDto>>();
        page!.Items.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task Search_AnotherOrganisation_SeesNoPolicies()
    {
        using var client = _factory.CreateClientFor(Guid.NewGuid(), Guid.NewGuid(), "Handler");

        var response = await client.GetAsync("/api/policies/search");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<PolicyDto>>();
        page!.Items.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task Search_PageSizeAboveTheCap_Answers422()
    {
        using var client = CreateSeedClient();

        var response = await client.GetAsync("/api/policies/search?pageSize=101");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    private HttpClient CreateSeedClient()
    {
        return _factory.CreateClientFor(Guid.NewGuid(), ApiFactory.SeedOrganisationId, "Handler");
    }
}
