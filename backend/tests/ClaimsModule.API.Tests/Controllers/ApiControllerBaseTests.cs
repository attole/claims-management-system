using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Controllers;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.Results;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class ApiControllerBaseTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    public async Task QueryAsync_Success_ReturnsTheDataItselfWithoutAnEnvelope()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/test/read");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("7");
    }

    [ApiFact]
    public async Task QueryAsync_APagedResult_ReturnsItsItemsAndCounts()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/test/paged");

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"items\":[\"a\",\"b\"]").And.Contain("\"totalCount\":51");
        json.Should().Contain("\"page\":2").And.Contain("\"pageSize\":25");
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task QueryAsync_Failure_ReturnsTheErrorBodyWithTheKindStatus()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/test/errors/Conflict");

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        body!.Type.Should().Be("ConflictError");
        body.Errors["Name"].Should().Equal("first message", "second message");
        body.Errors[ErrorBody.GeneralKey].Should().Equal("general message");
    }

    [ApiFact]
    public async Task CommandAsync_SuccessWithAWarning_ReturnsTheEnvelopeWithTheWarning()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/test/write-created", content: null);

        var body = await response.Content.ReadFromJsonAsync<ApiControllerBase.WriteEnvelope<int>>();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body!.Data.Should().Be(1);
        body.Warnings.Should().Equal(new Warning("TEST_WARNING", "be careful", "Notes"));
    }

    [ApiFact]
    public async Task CommandAsync_SuccessWithoutWarnings_ReturnsTheEnvelopeWithAnEmptyList()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/test/write-plain", content: null);

        var body = await response.Content.ReadFromJsonAsync<ApiControllerBase.WriteEnvelope<int>>();
        body!.Warnings.Should().BeEmpty();
    }

    [ApiFact]
    public async Task CommandAsync_ActionDeclaringAnotherSuccessStatus_UsesThatStatus()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/test/write-accepted", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [ApiFact]
    public async Task CommandAsync_ActionWithoutASuccessStatus_Answers200()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/test/write-plain", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [ApiFact]
    public async Task CommandAsync_ActionDeclaringTwoSuccessStatuses_FailsLoudly()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/test/write-ambiguous", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClientFor(Guid.NewGuid(), Guid.NewGuid(), "Handler");
    }
}
