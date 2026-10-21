using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClaimsModule.API.DevAuth;
using ClaimsModule.API.DevAuth.Configuration;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.API.Tests.Support.TestEndpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ClaimsModule.API.Tests.DevAuth;

[Collection(ApiCollection.Name)]
public sealed class DevAuthControllerTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);
    private readonly ApiFactory _disabledFactory = new(database, isDevAuthEnabled: false);

    public void Dispose()
    {
        _factory.Dispose();
        _disabledFactory.Dispose();
    }

    [ApiFact]
    public async Task Create_ConfiguredUser_ReturnsATokenForThatUser()
    {
        using var client = CreateClient(ApiFactory.DevAuthSecret);

        var response = await client.PostAsJsonAsync("/api/dev/token", new DevTokenRequest(ApiFactory.SupervisorId));

        var body = await response.Content.ReadFromJsonAsync<DevTokenDto>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Role.Should().Be("Supervisor");
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task Create_ConfiguredUser_ReturnsATokenTheApiAccepts()
    {
        using var client = CreateClient(ApiFactory.DevAuthSecret);
        var response = await client.PostAsJsonAsync("/api/dev/token", new DevTokenRequest(ApiFactory.ManagerId));
        var token = (await response.Content.ReadFromJsonAsync<DevTokenDto>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var whoami = await client.GetAsync("/test/whoami");

        var body = await whoami.Content.ReadFromJsonAsync<WhoAmIDto>();
        body.Should().BeEquivalentTo(new
        {
            Id = ApiFactory.ManagerId,
            OrganisationId = ApiFactory.SeedOrganisationId,
            Role = "Manager",
        });
    }

    [ApiFact]
    public async Task Create_UnknownUser_Answers404()
    {
        using var client = CreateClient(ApiFactory.DevAuthSecret);

        var response = await client.PostAsJsonAsync("/api/dev/token", new DevTokenRequest(Guid.NewGuid()));

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body!.Type.Should().Be("NotFoundError");
    }

    [ApiFact]
    public async Task Create_WhenDevAuthIsDisabled_Answers404()
    {
        using var client = _disabledFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/dev/token", new DevTokenRequest(ApiFactory.HandlerId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiTheory]
    [InlineData("")]
    [InlineData("wrong-devauth-secret-0123456789")]
    public async Task Create_WithoutTheConfiguredSecret_Answers403(string secret)
    {
        using var client = CreateClient(secret);

        var response = await client.PostAsJsonAsync("/api/dev/token", new DevTokenRequest(ApiFactory.HandlerId));

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        body!.Type.Should().Be("ForbiddenError");
    }

    [ApiFact]
    public async Task Create_UnknownUserWithoutTheSecret_Answers403SoUserIdsAreNotRevealed()
    {
        using var client = CreateClient(string.Empty);

        var response = await client.PostAsJsonAsync("/api/dev/token", new DevTokenRequest(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [ApiFact]
    public async Task Create_InDevelopmentWithoutASecret_NeedsNoHeader()
    {
        var noSecret = new Dictionary<string, string?> { ["DevAuth:Secret"] = string.Empty };
        using var development = _factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Development")
            .ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(noSecret)));
        using var client = development.CreateClient();

        var response = await client.PostAsJsonAsync("/api/dev/token", new DevTokenRequest(ApiFactory.HandlerId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private HttpClient CreateClient(string secret)
    {
        var client = _factory.CreateClient();

        if (secret.Length > 0)
        {
            client.DefaultRequestHeaders.Add(DevAuthOptions.SecretHeader, secret);
        }

        return client;
    }
}
