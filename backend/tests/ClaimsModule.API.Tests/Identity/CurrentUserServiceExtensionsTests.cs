using System.Net.Http.Json;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.API.Tests.Support.TestEndpoints;

namespace ClaimsModule.API.Tests.Identity;

[Collection(ApiCollection.Name)]
public sealed class CurrentUserServiceExtensionsTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    public async Task UseCurrentUser_TokenOfAUser_ShowsThatUserToTheHandlers()
    {
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        using var client = _factory.CreateClientFor(userId, organisationId, "Supervisor");

        var response = await client.GetAsync("/test/whoami");

        var body = await response.Content.ReadFromJsonAsync<WhoAmIDto>();
        body!.Id.Should().Be(userId);
        body.OrganisationId.Should().Be(organisationId);
        body.Role.Should().Be("Supervisor");
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task UseCurrentUser_SeededOrganisation_SeesTheSeededPolicies()
    {
        using var client = _factory.CreateClientFor(Guid.NewGuid(), ApiFactory.SeedOrganisationId, "Handler");

        var response = await client.GetAsync("/test/policies");

        var count = await response.Content.ReadFromJsonAsync<int>();
        count.Should().BeGreaterThan(0);
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task UseCurrentUser_AnotherOrganisation_SeesNoPolicies()
    {
        using var client = _factory.CreateClientFor(Guid.NewGuid(), Guid.NewGuid(), "Handler");

        var response = await client.GetAsync("/test/policies");

        var count = await response.Content.ReadFromJsonAsync<int>();
        count.Should().Be(0);
    }
}
