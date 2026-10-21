using System.Text;
using ClaimsModule.API.Pipeline.Idempotency;
using Microsoft.AspNetCore.Http;

namespace ClaimsModule.API.Tests.Pipeline.Idempotency;

public sealed class RequestFingerprintTests
{
    [Fact]
    public async Task ComputeAsync_SameJsonBody_GivesTheSameHash()
    {
        var first = await HashJsonAsync("{\"text\":\"a\"}");
        var second = await HashJsonAsync("{\"text\":\"a\"}");

        second.Should().Be(first);
    }

    [Fact]
    public async Task ComputeAsync_AnotherJsonBody_GivesAnotherHash()
    {
        var first = await HashJsonAsync("{\"text\":\"a\"}");
        var second = await HashJsonAsync("{\"text\":\"b\"}");

        second.Should().NotBe(first);
    }

    [Fact]
    public async Task ComputeAsync_AfterHashingTheBody_RewindsItForTheNextReader()
    {
        var request = CreateJsonRequest("{\"text\":\"a\"}");

        await RequestFingerprint.ComputeAsync(request, CancellationToken.None);

        request.Body.Position.Should().Be(0);
    }

    [Fact]
    public async Task ComputeAsync_SameMultipartContentWithAnotherBoundary_GivesTheSameHash()
    {
        var first = await HashMultipartAsync("boundary-one", "note", "report.pdf", [1, 2, 3]);
        var second = await HashMultipartAsync("boundary-two", "note", "report.pdf", [1, 2, 3]);

        second.Should().Be(first);
    }

    [Fact]
    public async Task ComputeAsync_AnotherFileContent_GivesAnotherHash()
    {
        var first = await HashMultipartAsync("boundary", "note", "report.pdf", [1, 2, 3]);
        var second = await HashMultipartAsync("boundary", "note", "report.pdf", [1, 2, 4]);

        second.Should().NotBe(first);
    }

    [Fact]
    public async Task ComputeAsync_AnotherFileName_GivesAnotherHash()
    {
        var first = await HashMultipartAsync("boundary", "note", "report.pdf", [1, 2, 3]);
        var second = await HashMultipartAsync("boundary", "note", "other.pdf", [1, 2, 3]);

        second.Should().NotBe(first);
    }

    [Fact]
    public async Task ComputeAsync_AnotherFormField_GivesAnotherHash()
    {
        var first = await HashMultipartAsync("boundary", "note one", "report.pdf", [1, 2, 3]);
        var second = await HashMultipartAsync("boundary", "note two", "report.pdf", [1, 2, 3]);

        second.Should().NotBe(first);
    }

    [Fact]
    public async Task ComputeAsync_TwoValuesOfAFieldAndOneValueWithAComma_GiveDifferentHashes()
    {
        var twoValues = await HashUrlEncodedAsync("x=a&x=b");
        var oneValue = await HashUrlEncodedAsync("x=a%2Cb");

        oneValue.Should().NotBe(twoValues);
    }

    [Fact]
    public async Task ComputeAsync_AnEmptyFileAndATextFieldWithTheSameNames_GiveDifferentHashes()
    {
        using var file = new MultipartFormDataContent("boundary");
        file.Add(new ByteArrayContent([]), "f", "x");
        using var field = new MultipartFormDataContent("boundary");
        field.Add(new StringContent("x"), "f");

        var fileHash = await HashContentAsync(file);
        var fieldHash = await HashContentAsync(field);

        fieldHash.Should().NotBe(fileHash);
    }

    private static async Task<string> HashUrlEncodedAsync(string body)
    {
        using var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

        return await HashContentAsync(content);
    }

    private static async Task<string> HashContentAsync(HttpContent content)
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = content.Headers.ContentType!.ToString();
        context.Request.Body = await content.ReadAsStreamAsync();

        return await RequestFingerprint.ComputeAsync(context.Request, CancellationToken.None);
    }

    private static HttpRequest CreateJsonRequest(string json)
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

        return context.Request;
    }

    private static async Task<string> HashJsonAsync(string json)
    {
        return await RequestFingerprint.ComputeAsync(CreateJsonRequest(json), CancellationToken.None);
    }

    private static async Task<string> HashMultipartAsync(string boundary, string note, string fileName, byte[] bytes)
    {
        using var content = new MultipartFormDataContent(boundary);
        content.Add(new StringContent(note), "note");
        content.Add(new ByteArrayContent(bytes), "file", fileName);
        var context = new DefaultHttpContext();
        context.Request.ContentType = content.Headers.ContentType!.ToString();
        context.Request.Body = await content.ReadAsStreamAsync();

        return await RequestFingerprint.ComputeAsync(context.Request, CancellationToken.None);
    }
}
