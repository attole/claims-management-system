using System.Net;
using ClaimsModule.API.Tests.Support;

namespace ClaimsModule.API.Tests.Swagger;

[Collection(ApiCollection.Name)]
public sealed class SwaggerDocsServiceExtensionsTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    public async Task UseSwaggerDocs_WhenEnabled_ServesTheOpenApiDocumentWithoutAToken()
    {
        using var enabled = _factory.WithSettings(new Dictionary<string, string?> { ["Swagger:Enabled"] = "true" });
        using var client = enabled.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [ApiFact]
    public async Task UseSwaggerDocs_WhenEnabled_ServesTheUiWithoutAToken()
    {
        using var enabled = _factory.WithSettings(new Dictionary<string, string?> { ["Swagger:Enabled"] = "true" });
        using var client = enabled.CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
