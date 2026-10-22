using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Controllers;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.Claims.RiskObjects;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class ClaimRiskObjectsControllerTests(ApiFixture database) : IDisposable
{
    private static readonly object _kitchen = new { AssetType = "Property", AssetDescription = "Kitchen" };

    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-claims-risk-objects")]
    public async Task Add_ToADraft_Answers201WithTheRiskObject()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PostAsJsonAsync($"/api/claims/{claimId}/risk-objects", _kitchen);

        var envelope = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<RiskObjectDto>>();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        envelope!.Data.AssetDescription.Should().Be("Kitchen");
    }

    [ApiFact]
    [Trait("Rule", "S:API-PUT-claims-risk-objects")]
    public async Task Update_MarkAddedRiskObjectPrimary_Answers200WithThePrimary()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);
        var added = await client.PostAsJsonAsync($"/api/claims/{claimId}/risk-objects", _kitchen);
        var riskObject = await added.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<RiskObjectDto>>();
        var primary = new { AssetType = "Property", AssetDescription = "Kitchen", IsPrimary = true };

        var response = await client.PutAsJsonAsync(
            $"/api/claims/{claimId}/risk-objects/{riskObject!.Data.Id}",
            primary);

        var envelope = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<RiskObjectDto>>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        envelope!.Data.IsPrimary.Should().BeTrue();
    }

    [ApiFact]
    [Trait("Rule", "S:API-PUT-claims-risk-objects")]
    public async Task Update_UnknownRiskObject_Answers404()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PutAsJsonAsync($"/api/claims/{claimId}/risk-objects/{Guid.NewGuid()}", _kitchen);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiFact]
    [Trait("Rule", "S:API-DELETE-claims-risk-objects")]
    public async Task Remove_AnAddedRiskObject_Answers200()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);
        var added = await client.PostAsJsonAsync($"/api/claims/{claimId}/risk-objects", _kitchen);
        var riskObject = await added.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<RiskObjectDto>>();

        var response = await client.DeleteAsync($"/api/claims/{claimId}/risk-objects/{riskObject!.Data.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [ApiFact]
    [Trait("Rule", "S:API-DELETE-claims-risk-objects")]
    public async Task Remove_UnknownRiskObject_Answers404()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.DeleteAsync($"/api/claims/{claimId}/risk-objects/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private HttpClient CreateClient(Guid organisationId)
    {
        return _factory.CreateClientFor(Guid.NewGuid(), organisationId, "Handler");
    }
}
