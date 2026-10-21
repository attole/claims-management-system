using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Infrastructure.Storage.Local;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class DocumentDownloadControllerTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims-documents")]
    public async Task Download_WithTheLinkOfAnUpload_ReturnsTheFileWithoutAToken()
    {
        var link = await UploadAndGetLinkAsync("evidence.pdf", [1, 2, 3, 4]);
        using var anonymous = _factory.CreateClient();

        var response = await anonymous.GetAsync(new Uri(link).PathAndQuery);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        bytes.Should().Equal(1, 2, 3, 4);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims-documents")]
    public async Task Download_WithTheLinkOfAnUpload_NamesTheFileAfterTheOriginal()
    {
        var link = await UploadAndGetLinkAsync("evidence.pdf", [1, 2, 3, 4]);
        using var anonymous = _factory.CreateClient();

        var response = await anonymous.GetAsync(new Uri(link).PathAndQuery);

        response.Content.Headers.ContentDisposition!.FileName.Should().Be("evidence.pdf");
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims-documents")]
    public async Task Download_WithATamperedToken_Answers404()
    {
        var link = await UploadAndGetLinkAsync("evidence.pdf", [1, 2, 3, 4]);
        using var anonymous = _factory.CreateClient();

        var response = await anonymous.GetAsync(new Uri(link).PathAndQuery + "x");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims-documents")]
    public async Task Download_WithoutAToken_Answers422()
    {
        using var anonymous = _factory.CreateClient();

        var response = await anonymous.GetAsync("/api/documents/download");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims-documents")]
    public async Task Download_WhenTheProviderIsAzure_Answers404EvenForAValidToken()
    {
        var path = "claim-documents/org/claim/abc_evidence.pdf";
        var file = Path.Combine(_factory.StorageRoot, "org", "claim", "abc_evidence.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        await File.WriteAllBytesAsync(file, [1, 2, 3]);
        var azure = _factory.WithSettings(new Dictionary<string, string?>
        {
            ["StorageProvider"] = "AzureBlob",
            ["AzureConnectionString"] = "UseDevelopmentStorage=true",
        });
        var token = azure.Services.GetRequiredService<LocalDownloadToken>().Create(path, TimeSpan.FromHours(1));
        using var anonymous = azure.CreateClient();

        var response = await anonymous.GetAsync($"/api/documents/download?token={Uri.EscapeDataString(token)}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<string> UploadAndGetLinkAsync(string fileName, byte[] content)
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = _factory.CreateClientFor(Guid.NewGuid(), organisationId, "Handler");
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        var form = new MultipartFormDataContent
        {
            { file, "file", fileName },
            { new StringContent("Police report"), "documentType" },
        };
        await client.PostAsync($"/api/claims/{claimId}/documents", form);

        var links = await client.GetFromJsonAsync<List<ClaimDocumentLinkDto>>($"/api/claims/{claimId}/documents");

        return links![0].DownloadUrl;
    }
}
