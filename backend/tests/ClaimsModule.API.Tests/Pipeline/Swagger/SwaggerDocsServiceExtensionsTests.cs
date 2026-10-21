using System.Net;
using System.Text.Json;
using ClaimsModule.API.Tests.Support;

namespace ClaimsModule.API.Tests.Pipeline.Swagger;

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

    [ApiFact]
    public async Task UseSwaggerDocs_WriteActionWithADeclaredStatus_DocumentsItAndTheErrorResponses()
    {
        using var enabled = _factory.WithSettings(new Dictionary<string, string?> { ["Swagger:Enabled"] = "true" });
        using var client = enabled.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var responses = document.RootElement
            .GetProperty("paths")
            .GetProperty("/test/write-created")
            .GetProperty("post")
            .GetProperty("responses");
        var codes = responses.EnumerateObject().Select(property => property.Name).ToList();
        codes.Should().Contain(["201", "401", "403", "404", "409", "422"]);
    }

    [ApiFact]
    public async Task UseSwaggerDocs_WhenEnabled_DocumentsTheBearerSchemeAndTheDevTokenNote()
    {
        using var enabled = _factory.WithSettings(new Dictionary<string, string?> { ["Swagger:Enabled"] = "true" });
        using var client = enabled.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var schemes = document.RootElement.GetProperty("components").GetProperty("securitySchemes");
        var scheme = schemes.GetProperty("Bearer");
        scheme.GetProperty("scheme").GetString().Should().Be("bearer");
        document.RootElement.GetProperty("info").GetProperty("description").GetString().Should().Contain("simulates");
    }
}
