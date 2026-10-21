using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Controllers;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.Claims.Parties;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class ClaimPartiesControllerTests(ApiFixture database) : IDisposable
{
    private static readonly object _person = new { Role = "Claimant", Type = "Person", FirstName = "Ada" };

    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task Add_WithoutAToken_Answers401()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/claims/{Guid.NewGuid()}/parties", _person);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-claims-parties")]
    public async Task Add_APersonToADraft_Answers201WithTheParty()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PostAsJsonAsync($"/api/claims/{claimId}/parties", _person);

        var envelope = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<PartyDto>>();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        envelope!.Data.FirstName.Should().Be("Ada");
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task Add_APersonWithoutAName_Answers422()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);
        var body = new { Role = "Claimant", Type = "Person" };

        var response = await client.PostAsJsonAsync($"/api/claims/{claimId}/parties", body);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task Add_ToAClaimOfAnotherOrganisation_Answers404()
    {
        var (claimId, _) = await _factory.SeedClaimAsync(Guid.NewGuid());
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.PostAsJsonAsync($"/api/claims/{claimId}/parties", _person);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiFact]
    [Trait("Rule", "S:API-DELETE-claims-parties")]
    public async Task Remove_AnAddedParty_Answers200()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);
        var witness = new { Role = "Witness", Type = "Person", FirstName = "Ada" };
        var added = await client.PostAsJsonAsync($"/api/claims/{claimId}/parties", witness);
        var party = await added.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<PartyDto>>();

        var response = await client.DeleteAsync($"/api/claims/{claimId}/parties/{party!.Data.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [ApiFact]
    [Trait("Rule", "S:API-DELETE-claims-parties")]
    public async Task Remove_UnknownParty_Answers404()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.DeleteAsync($"/api/claims/{claimId}/parties/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private HttpClient CreateClient(Guid organisationId)
    {
        return _factory.CreateClientFor(Guid.NewGuid(), organisationId, "Handler");
    }
}
