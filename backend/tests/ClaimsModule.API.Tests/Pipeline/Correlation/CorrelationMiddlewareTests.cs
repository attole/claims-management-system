using System.Net.Http.Json;
using ClaimsModule.API.Pipeline.Correlation;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.API.Tests.Support.TestEndpoints;

namespace ClaimsModule.API.Tests.Pipeline.Correlation;

[Collection(ApiCollection.Name)]
public sealed class CorrelationMiddlewareTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:AUD-Correlation")]
    public async Task InvokeAsync_AnyRequest_ReturnsAGuidInTheCorrelationHeader()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/test/anonymous");

        var header = response.Headers.GetValues(CorrelationMiddleware.HeaderName).Single();
        Guid.TryParse(header, out _).Should().BeTrue();
    }

    [ApiFact]
    public async Task InvokeAsync_RequestCarryingACorrelationHeader_IgnoresIt()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test/anonymous");
        var forged = "9f3c1d52-4b1e-4c55-9a47-0d8f6a2b7e11";
        request.Headers.Add(CorrelationMiddleware.HeaderName, forged);

        var response = await client.SendAsync(request);

        var header = response.Headers.GetValues(CorrelationMiddleware.HeaderName).Single();
        header.Should().NotBe(forged);
    }

    [ApiFact]
    [Trait("Rule", "S:AUD-Correlation")]
    public async Task InvokeAsync_AuthenticatedRequest_ExposesTheSameIdToTheHandlers()
    {
        using var client = _factory.CreateClientFor(ApiFactory.HandlerId, ApiFactory.SeedOrganisationId, "Handler");

        var response = await client.GetAsync("/test/whoami");

        var body = await response.Content.ReadFromJsonAsync<WhoAmIDto>();
        var header = response.Headers.GetValues(CorrelationMiddleware.HeaderName).Single();
        body!.CorrelationId.ToString().Should().Be(header);
    }

    [ApiFact]
    public async Task InvokeAsync_TwoRequests_GetDifferentIds()
    {
        using var client = _factory.CreateClient();

        var first = await client.GetAsync("/test/anonymous");
        var second = await client.GetAsync("/test/anonymous");

        var firstId = first.Headers.GetValues(CorrelationMiddleware.HeaderName).Single();
        var secondId = second.Headers.GetValues(CorrelationMiddleware.HeaderName).Single();
        firstId.Should().NotBe(secondId);
    }
}
