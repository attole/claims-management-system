using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Pipeline.Identity;
using ClaimsModule.API.Tests.Support;
using Microsoft.Extensions.Options;

namespace ClaimsModule.API.Tests.Pipeline.Jwt;

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
    public async Task AddJwt_RequestWithoutAToken_AnswersWithTheErrorBodyAndAChallengeHeader()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/test/whoami");

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        body!.Type.Should().Be("UnauthorizedError");
        response.Headers.WwwAuthenticate.Should().ContainSingle(header => header.Scheme == "Bearer");
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task AddJwt_AuthenticatedUserNotAllowedByThePolicy_Answers403InTheErrorBody()
    {
        using var client = _factory.CreateClientFor(Guid.NewGuid(), Guid.NewGuid(), "Handler");

        var response = await client.GetAsync("/test/forbidden");

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        body!.Type.Should().Be("ForbiddenError");
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
    [Trait("Rule", "S:API-Auth")]
    public void AddJwt_SigningKeyIsAnUnresolvedKeyVaultReference_StopsTheApplicationAtStartup()
    {
        var reference = "@Microsoft.KeyVault(SecretUri=https://kv.vault.azure.net/secrets/jwt-signing-key/)";
        var settings = new Dictionary<string, string?> { ["Jwt:SigningKey"] = reference };
        using var broken = _factory.WithSettings(settings);

        var act = () => broken.CreateClient();

        act.Should().Throw<OptionsValidationException>().WithMessage("*Key Vault reference*");
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
