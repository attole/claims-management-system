using System.Net;
using ClaimsModule.API.Tests.Support;

namespace ClaimsModule.API.Tests;

[Collection(ApiCollection.Name)]
public sealed class ProgramTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    public async Task Pipeline_HangfireDashboardEnabled_IsReachableWithoutAToken()
    {
        var settings = new Dictionary<string, string?> { ["Hangfire:Dashboard:Enabled"] = "true" };
        using var enabled = _factory.WithSettings(settings);
        using var client = enabled.CreateClient();

        var response = await client.GetAsync("/hangfire");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [ApiFact]
    public async Task Pipeline_ControllerActionWithoutAToken_Answers401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/test/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
