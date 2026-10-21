using System.Net;
using System.Net.Http.Headers;
using ClaimsModule.API.Identity;
using ClaimsModule.API.Tests.Support;

namespace ClaimsModule.API.Tests.Jwt;

[Collection(ApiCollection.Name)]
public sealed class JwtServiceExtensionsTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task AddJwt_RequestWithoutAToken_Answers401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/test/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task AddJwt_TokenSignedWithAnotherKey_Answers401()
    {
        using var client = _factory.CreateClient();
        var token = TestTokens.Create("another-signing-key-that-is-long-enough-0123456789", ValidClaims());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/test/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task AddJwt_TokenWithoutTheOrganisationClaim_Answers401()
    {
        using var client = _factory.CreateClient();
        var claims = ValidClaims();
        claims.Remove(IdentityClaims.Organisation);
        var token = TestTokens.Create(TestTokens.SigningKey, claims);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/test/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task AddJwt_ValidToken_IsAccepted()
    {
        using var client = _factory.CreateClientFor(Guid.NewGuid(), Guid.NewGuid(), "Handler");

        var response = await client.GetAsync("/test/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [ApiFact]
    public async Task AddJwt_ActionMarkedAnonymous_NeedsNoToken()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/test/anonymous");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static Dictionary<string, object> ValidClaims()
    {
        return new Dictionary<string, object>
        {
            [IdentityClaims.Subject] = Guid.NewGuid().ToString(),
            [IdentityClaims.Organisation] = Guid.NewGuid().ToString(),
            [IdentityClaims.Role] = "Handler",
            [IdentityClaims.Name] = "Test User",
        };
    }
}
