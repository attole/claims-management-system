using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.ReserveComponents.Dtos;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class ClaimReservesControllerTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task Submit_WithoutAToken_Answers401()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/claims/{Guid.NewGuid()}/reserves", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-reserves")]
    public async Task Get_ClaimWithoutReserves_ReturnsAnEmptySummary()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims/{claimId}/reserves");

        var reserves = await response.Content.ReadFromJsonAsync<ClaimReservesDto>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        reserves!.Components.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-reserves")]
    public async Task Get_UnknownClaim_Answers404()
    {
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync($"/api/claims/{Guid.NewGuid()}/reserves");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task Submit_UnknownTransactionTypeName_Answers422()
    {
        using var client = CreateClient(Guid.NewGuid());
        var body = new { TransactionType = "Bogus", Amount = 100, ChangeReason = "Why", ComponentType = "Indemnity" };

        var response = await client.PostAsJsonAsync($"/api/claims/{Guid.NewGuid()}/reserves", body);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-reserves")]
    public async Task Submit_ToAClaimOfAnotherOrganisation_Answers404()
    {
        var (claimId, _) = await _factory.SeedClaimAsync(Guid.NewGuid());
        using var client = CreateClient(Guid.NewGuid());
        var body = new { TransactionType = "Add", Amount = 100, ChangeReason = "Initial", ComponentType = "Indemnity" };

        var response = await client.PostAsJsonAsync($"/api/claims/{claimId}/reserves", body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiTheory]
    [Trait("Rule", "S:API-POST-reserve-approve")]
    [Trait("Rule", "S:API-POST-reserve-retract")]
    [InlineData("approve")]
    [InlineData("retract")]
    [InlineData("retry-posting")]
    public async Task Decide_UnknownReserveHistory_Answers404(string action)
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PostAsync($"/api/claims/{claimId}/reserves/{Guid.NewGuid()}/{action}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-reserve-reject")]
    public async Task Reject_UnknownReserveHistory_Answers404()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PostAsJsonAsync(
            $"/api/claims/{claimId}/reserves/{Guid.NewGuid()}/reject",
            new { RejectionReason = "Not covered" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private HttpClient CreateClient(Guid organisationId)
    {
        return _factory.CreateClientFor(Guid.NewGuid(), organisationId, "Supervisor");
    }
}
