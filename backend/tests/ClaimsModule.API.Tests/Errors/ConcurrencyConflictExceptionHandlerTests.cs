using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Tests.Support;

namespace ClaimsModule.API.Tests.Errors;

[Collection(ApiCollection.Name)]
public sealed class ConcurrencyConflictExceptionHandlerTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    public async Task TryHandleAsync_ConcurrencyConflict_Answers409InTheErrorBody()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/test/conflict");

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        body!.Type.Should().Be("ConflictError");
    }

    [ApiFact]
    public async Task TryHandleAsync_ConcurrencyConflict_DoesNotLeakTheExceptionText()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/test/conflict");

        (await response.Content.ReadAsStringAsync()).Should().NotContain("inner").And.NotContain("test conflict");
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClientFor(Guid.NewGuid(), Guid.NewGuid(), "Handler");
    }
}
