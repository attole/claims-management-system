using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ClaimsModule.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Storage.Azure;

internal sealed class AzureBlobStorageService(IOptions<StorageOptions> options, IClock clock) : IStorageService
{
    private readonly BlobServiceClient _client = new(options.Value.AzureConnectionString);

    public async Task UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var target = StoragePath.From(path);
        var container = _client.GetBlobContainerClient(target.Container);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var upload = new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } };
        await container.GetBlobClient(target.Name).UploadAsync(content, upload, cancellationToken);
    }

    public Task<string> GetDownloadUrlAsync(string path, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        var target = StoragePath.From(path);
        var blob = _client.GetBlobContainerClient(target.Container).GetBlobClient(target.Name);
        var url = blob.GenerateSasUri(BlobSasPermissions.Read, clock.UtcNow.Add(lifetime));

        return Task.FromResult(url.ToString());
    }
}
