using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using ClaimsModule.API.Controllers;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class ClaimDocumentsControllerTests(ApiFixture database) : IDisposable
{
    private const int Megabyte = 1024 * 1024;

    private readonly ApiFactory _factory = new(database);
    private readonly Lazy<KestrelApiFactory> _kestrel = new(() => new KestrelApiFactory(database));

    public void Dispose()
    {
        _factory.Dispose();

        if (_kestrel.IsValueCreated)
        {
            _kestrel.Value.Dispose();
        }
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-claims-documents")]
    public async Task Upload_APdf_Answers201WithTheDocument()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PostAsync($"/api/claims/{claimId}/documents", Form(2048, "report.pdf"));

        var envelope = await ReadDocumentAsync(response);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        envelope!.Data.FileSizeBytes.Should().Be(2048);
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-claims-documents")]
    public async Task Upload_WithEmptyNotes_Answers201()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);
        var form = Form(2048, "report.pdf");
        form.Add(new StringContent(string.Empty), "notes");

        var response = await client.PostAsync($"/api/claims/{claimId}/documents", form);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task Upload_WithoutAFile_Answers422()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);
        var form = new MultipartFormDataContent { { new StringContent("Police report"), "documentType" } };

        var response = await client.PostAsync($"/api/claims/{claimId}/documents", form);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:DOC-MimeAllowlist")]
    public async Task Upload_AnExecutable_Answers422()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PostAsync(
            $"/api/claims/{claimId}/documents",
            Form(10, "tool.exe", "application/x-msdownload"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    [Trait("Rule", "S:DOC-MaxSize")]
    public void Upload_Action_CarriesARequestSizeLimit()
    {
        var upload = typeof(ClaimDocumentsController).GetMethod(nameof(ClaimDocumentsController.Upload))!;

        var result = upload.GetCustomAttribute<RequestSizeLimitAttribute>();

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Rule", "S:DOC-MaxSize")]
    public void Upload_Action_RaisesTheFormLimitAboveTheDomainLimit()
    {
        var upload = typeof(ClaimDocumentsController).GetMethod(nameof(ClaimDocumentsController.Upload))!;

        var result = upload.GetCustomAttribute<RequestFormLimitsAttribute>();

        result!.MultipartBodyLengthLimit.Should().BeGreaterThan(50 * Megabyte);
    }

    [ApiFact]
    [Trait("Rule", "S:DOC-MaxSize")]
    public async Task Upload_JustOverTheLimit_Answers422FromTheDomainRule()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PostAsync($"/api/claims/{claimId}/documents", Form((50 * Megabyte) + 1, "big.pdf"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:DOC-MaxSize")]
    public async Task Upload_OverTheServerLimitOnKestrel_IsRefusedAsATooLargeBody()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = _kestrel.Value.CreateKestrelClientFor(Guid.NewGuid(), organisationId, "Handler");

        var response = await client.PostAsync($"/api/claims/{claimId}/documents", Form(52 * Megabyte, "huge.pdf"));

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("Request body too large");
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task Upload_JustUnderTheLimitOnKestrelWithAKey_ReachesTheAction()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = _kestrel.Value.CreateKestrelClientFor(Guid.NewGuid(), organisationId, "Handler");

        var response = await PostWithKeyAsync(client, claimId, Form((50 * Megabyte) - 1024, "large.pdf"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task Upload_JustUnderTheLimitWithAKey_ReachesTheAction()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await PostWithKeyAsync(client, claimId, Form((50 * Megabyte) - 1024, "large.pdf"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task Upload_RepeatedWithTheSameKey_ReturnsTheFirstDocument()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);
        var key = Guid.NewGuid().ToString();
        var first = await PostWithKeyAsync(client, claimId, Form(512, "same.pdf"), key);
        var firstDocument = await ReadDocumentAsync(first);

        var second = await PostWithKeyAsync(client, claimId, Form(512, "same.pdf"), key);

        var secondDocument = await ReadDocumentAsync(second);
        secondDocument!.Data.Id.Should().Be(firstDocument!.Data.Id);
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task Upload_ToAClaimOfAnotherOrganisation_Answers404()
    {
        var (claimId, _) = await _factory.SeedClaimAsync(Guid.NewGuid());
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.PostAsync($"/api/claims/{claimId}/documents", Form(10, "a.pdf"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims-documents")]
    public async Task List_AfterAnUpload_ReturnsTheDocumentWithADownloadUrl()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);
        await client.PostAsync($"/api/claims/{claimId}/documents", Form(64, "list.pdf"));

        var response = await client.GetAsync($"/api/claims/{claimId}/documents");

        var links = await response.Content.ReadFromJsonAsync<List<ClaimDocumentLinkDto>>();
        links!.Select(link => link.Document.DocumentName).Should().Equal("list.pdf");
        links![0].DownloadUrl.Should().Contain("/api/documents/download?token=");
    }

    private static async Task<ApiControllerBase.CommandResponse<ClaimDocumentDto>?> ReadDocumentAsync(
        HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ClaimDocumentDto>>();
    }

    private static MultipartFormDataContent Form(int size, string fileName, string contentType = "application/pdf")
    {
        var content = new byte[size];
        "%PDF"u8.CopyTo(content);
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        return new MultipartFormDataContent
        {
            { file, "file", fileName },
            { new StringContent("Police report"), "documentType" },
        };
    }

    private static async Task<HttpResponseMessage> PostWithKeyAsync(
        HttpClient client,
        Guid claimId,
        MultipartFormDataContent form,
        string? key = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/claims/{claimId}/documents") { Content = form };
        request.Headers.Add("Idempotency-Key", key ?? Guid.NewGuid().ToString());

        return await client.SendAsync(request);
    }

    private HttpClient CreateClient(Guid organisationId)
    {
        return _factory.CreateClientFor(Guid.NewGuid(), organisationId, "Handler");
    }
}
