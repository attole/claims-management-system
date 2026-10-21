using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Controllers;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Claims.Issues;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class ClaimIssuesControllerTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-claims-issues-acknowledge")]
    public async Task Acknowledge_AWarningOfTheClaim_Answers200WithTheIssue()
    {
        using var client = CreateClient();
        var claimId = await CreateDraftWithoutPolicyAsync(client);
        var detail = await client.GetFromJsonAsync<ClaimDetailDto>($"/api/claims/{claimId}");
        var warning = detail!.Issues.First(issue => issue.Severity == "Warning");

        var response = await client.PostAsync($"/api/claims/{claimId}/issues/{warning.Id}/acknowledge", null);

        var envelope = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<IssueDto>>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        envelope!.Data.Id.Should().Be(warning.Id);
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-claims-issues-acknowledge")]
    public async Task Acknowledge_UnknownIssue_Answers404()
    {
        using var client = CreateClient();
        var claimId = await CreateDraftWithoutPolicyAsync(client);

        var response = await client.PostAsync($"/api/claims/{claimId}/issues/{Guid.NewGuid()}/acknowledge", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-claims-issues-waive")]
    public async Task Waive_UnknownIssue_Answers404()
    {
        using var client = CreateClient();
        var claimId = await CreateDraftWithoutPolicyAsync(client);

        var response = await client.PostAsync($"/api/claims/{claimId}/issues/{Guid.NewGuid()}/waive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<Guid> CreateDraftWithoutPolicyAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/claims", new { ReportDate = DateTimeOffset.UtcNow });
        var envelope = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ClaimDto>>();

        return envelope!.Data.Id;
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClientFor(ApiFactory.HandlerId, ApiFactory.SeedOrganisationId, "Handler");
    }
}
