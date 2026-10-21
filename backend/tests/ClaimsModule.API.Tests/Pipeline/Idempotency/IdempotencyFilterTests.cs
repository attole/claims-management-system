using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Pipeline.Idempotency;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.API.Tests.Support.TestEndpoints;

namespace ClaimsModule.API.Tests.Pipeline.Idempotency;

[Collection(ApiCollection.Name)]
public sealed class IdempotencyFilterTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_RequestWithoutAKey_RunsEveryTime()
    {
        using var client = CreateClient();

        var first = await Echo(client, "text", key: null);
        var second = await Echo(client, "text", key: null);

        second.Calls.Should().BeGreaterThan(first.Calls);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_SameKeyAndBody_ReplaysTheFirstResponseWithoutRunningAgain()
    {
        using var client = CreateClient();
        var key = NewKey();

        var first = await Echo(client, "text", key);
        var second = await Echo(client, "text", key);

        second.Should().Be(first);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_Replay_KeepsTheStatusCodeOfTheFirstResponse()
    {
        using var client = CreateClient();
        var key = NewKey();
        var first = await Send(client, HttpMethod.Post, "/test/echo-created", new { text = "x" }, key);
        var firstBody = await first.Content.ReadFromJsonAsync<TestController.EchoResponse>();

        var replay = await Send(client, HttpMethod.Post, "/test/echo-created", new { text = "x" }, key);

        var replayBody = await replay.Content.ReadFromJsonAsync<TestController.EchoResponse>();
        replay.StatusCode.Should().Be(HttpStatusCode.Created);
        replayBody.Should().Be(firstBody);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_SameKeyWithAnotherBody_Answers422()
    {
        using var client = CreateClient();
        var key = NewKey();
        await Echo(client, "first", key);

        var response = await Send(client, HttpMethod.Post, "/test/echo", new { text = "second" }, key);

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body!.Type.Should().Be("ValidationError");
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_KeyLongerThanTheColumn_Answers422()
    {
        using var client = CreateClient();
        var key = new string('k', IdempotencyFilter.MaxKeyLength + 1);

        var response = await Send(client, HttpMethod.Post, "/test/echo", new { text = "x" }, key);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_KeyOfTheLongestAllowedLength_IsAccepted()
    {
        using var client = CreateClient();
        var key = new string('k', IdempotencyFilter.MaxKeyLength);

        var response = await Send(client, HttpMethod.Post, "/test/echo", new { text = "x" }, key);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_InterruptedRequest_AnswersConflictForTheSameKey()
    {
        using var client = CreateClient();
        var key = NewKey();
        await Send(client, HttpMethod.Post, "/test/echo-crash", new { text = "x" }, key);

        var response = await Send(client, HttpMethod.Post, "/test/echo-crash", new { text = "x" }, key);

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        body!.Type.Should().Be("ConflictError");
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_RefusedRequest_IsNotStoredSoTheSameKeyRunsAgain()
    {
        using var client = CreateClient();
        var key = NewKey();
        await Send(client, HttpMethod.Post, "/test/echo-refused", body: null, key);
        var callsAfterFirst = TestController.EchoCalls;

        var retry = await Send(client, HttpMethod.Post, "/test/echo-refused", body: null, key);

        retry.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        TestController.EchoCalls.Should().BeGreaterThan(callsAfterFirst);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_MultipartUpload_IsReplayedLikeAnyWrite()
    {
        using var client = CreateClient();
        var key = NewKey();

        var first = await Upload(client, key, "report.pdf", [1, 2, 3]);
        var second = await Upload(client, key, "report.pdf", [1, 2, 3]);

        second.Should().Be(first);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_MultipartUploadWithAnotherFile_Answers422()
    {
        using var client = CreateClient();
        var key = NewKey();
        await Upload(client, key, "report.pdf", [1, 2, 3]);
        using var content = CreateUpload("report.pdf", [9, 9, 9]);
        using var request = CreateRequest(HttpMethod.Post, "/test/upload", key);
        request.Content = content;

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_PutRequest_IsReplayedToo()
    {
        using var client = CreateClient();
        var key = NewKey();
        var first = await Send(client, HttpMethod.Put, "/test/echo-put", new { text = "x" }, key);
        var firstBody = await first.Content.ReadFromJsonAsync<TestController.EchoResponse>();

        var second = await Send(client, HttpMethod.Put, "/test/echo-put", new { text = "x" }, key);

        var secondBody = await second.Content.ReadFromJsonAsync<TestController.EchoResponse>();
        secondBody.Should().Be(firstBody);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_ReplayOfAResponseWithoutABody_DoesNotRunTheActionAgain()
    {
        using var client = CreateClient();
        var key = NewKey();
        await Send(client, HttpMethod.Post, "/test/echo-no-content", body: null, key);
        var callsAfterFirst = TestController.EchoCalls;

        var replay = await Send(client, HttpMethod.Post, "/test/echo-no-content", body: null, key);

        replay.StatusCode.Should().Be(HttpStatusCode.NoContent);
        TestController.EchoCalls.Should().Be(callsAfterFirst);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_ReadRequestWithAKey_IsIgnored()
    {
        using var client = CreateClient();
        var key = NewKey();

        var first = await Send(client, HttpMethod.Get, "/test/echo-get", body: null, key);
        var second = await Send(client, HttpMethod.Get, "/test/echo-get", body: null, key);

        var firstBody = await first.Content.ReadFromJsonAsync<TestController.EchoResponse>();
        var secondBody = await second.Content.ReadFromJsonAsync<TestController.EchoResponse>();
        secondBody!.Calls.Should().BeGreaterThan(firstBody!.Calls);
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task OnResourceExecutionAsync_SameKeyFromAnotherUser_RunsAgain()
    {
        using var first = CreateClient();
        using var second = CreateClient();
        var key = NewKey();

        var firstResult = await Echo(first, "text", key);
        var secondResult = await Echo(second, "text", key);

        secondResult.Calls.Should().BeGreaterThan(firstResult.Calls);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task OnResourceExecutionAsync_ReplayOfAnEmptyOkResult_KeepsTheStatusAndDoesNotRunAgain()
    {
        using var client = CreateClient();
        var key = NewKey();
        var first = await Send(client, HttpMethod.Post, "/test/echo-empty", body: null, key);
        var callsAfterFirst = TestController.EchoCalls;

        var replay = await Send(client, HttpMethod.Post, "/test/echo-empty", body: null, key);

        replay.StatusCode.Should().Be(first.StatusCode);
        TestController.EchoCalls.Should().Be(callsAfterFirst);
    }

    private static string NewKey()
    {
        return Guid.NewGuid().ToString("N");
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string? key)
    {
        var request = new HttpRequestMessage(method, url);

        if (key is not null)
        {
            request.Headers.Add(IdempotencyFilter.HeaderName, key);
        }

        return request;
    }

    private static MultipartFormDataContent CreateUpload(string fileName, byte[] bytes)
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(bytes), "file", fileName);

        return content;
    }

    private static async Task<HttpResponseMessage> Send(
        HttpClient client,
        HttpMethod method,
        string url,
        object? body,
        string key)
    {
        using var request = CreateRequest(method, url, key);
        request.Content = body is null ? null : JsonContent.Create(body);

        return await client.SendAsync(request);
    }

    private static async Task<TestController.EchoResponse> Echo(HttpClient client, string text, string? key)
    {
        using var request = CreateRequest(HttpMethod.Post, "/test/echo", key);
        request.Content = JsonContent.Create(new { text });

        var response = await client.SendAsync(request);

        return (await response.Content.ReadFromJsonAsync<TestController.EchoResponse>())!;
    }

    private static async Task<TestController.EchoResponse> Upload(
        HttpClient client,
        string key,
        string fileName,
        byte[] bytes)
    {
        using var content = CreateUpload(fileName, bytes);
        using var request = CreateRequest(HttpMethod.Post, "/test/upload", key);
        request.Content = content;

        var response = await client.SendAsync(request);

        return (await response.Content.ReadFromJsonAsync<TestController.EchoResponse>())!;
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClientFor(Guid.NewGuid(), Guid.NewGuid(), "Handler");
    }
}
