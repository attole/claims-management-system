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

    public async Task<IReadOnlyList<string>> ListAsync(
        string container,
        DateTimeOffset createdBefore,
        CancellationToken cancellationToken)
    {
        var containerClient = _client.GetBlobContainerClient(container);
        var paths = new List<string>();

        if (!await containerClient.ExistsAsync(cancellationToken))
        {
            return paths;
        }

        await foreach (var blob in containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            if (blob.Properties.LastModified < createdBefore)
            {
                paths.Add($"{container}/{blob.Name}");
            }
        }

        return paths;
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        var target = StoragePath.From(path);
        var container = _client.GetBlobContainerClient(target.Container);

        await container.DeleteBlobIfExistsAsync(target.Name, cancellationToken: cancellationToken);
    }

    public Task<string> GetDownloadUrlAsync(string path, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        var target = StoragePath.From(path);
        var blob = _client.GetBlobContainerClient(target.Container).GetBlobClient(target.Name);
        var url = blob.GenerateSasUri(BlobSasPermissions.Read, clock.UtcNow.Add(lifetime));

        return Task.FromResult(url.ToString());
    }
}
