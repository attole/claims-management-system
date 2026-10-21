using System.Net;
using System.Net.Http.Json;
using System.Text;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Tests.Support;

namespace ClaimsModule.API.Tests.Errors;

[Collection(ApiCollection.Name)]
public sealed class ErrorsServiceExtensionsTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task AddErrors_RequestMissingARequiredField_Answers422InTheErrorBody()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/test/bind", new { });

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body!.Type.Should().Be("ValidationError");
        body.Errors.Keys.Should().Contain("Name");
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task AddErrors_RequestThatIsNotJson_Answers422NotAnOwn400()
    {
        using var client = CreateClient();
        using var content = new StringContent("{ not json", Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/test/bind", content);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    public async Task AddErrors_RequestWithoutABody_Answers422UnderTheGeneralKey()
    {
        using var client = CreateClient();
        using var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/test/bind", content);

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body!.Errors.Keys.Should().Contain(ErrorBody.GeneralKey);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClientFor(Guid.NewGuid(), Guid.NewGuid(), "Handler");
    }
}
