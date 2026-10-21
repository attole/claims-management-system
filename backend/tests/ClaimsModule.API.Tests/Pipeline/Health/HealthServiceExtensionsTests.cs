using System.Net;
using ClaimsModule.API.Pipeline.Health;
using ClaimsModule.API.Tests.Support;

namespace ClaimsModule.API.Tests.Pipeline.Health;

[Collection(ApiCollection.Name)]
public sealed class HealthServiceExtensionsTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    public async Task UseHealth_LivePathWithoutAToken_Answers200()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(HealthServiceExtensions.LivePath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [ApiFact]
    public async Task UseHealth_ReadyPathWithAReachableDatabase_Answers200WithoutAToken()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(HealthServiceExtensions.ReadyPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
