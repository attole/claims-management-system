using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Storage.Local;

public sealed class LocalDocumentDownloads(
    LocalDownloadToken tokens,
    LocalFileSystemStorageService storage,
    IOptions<StorageOptions> options) : ILocalDocumentDownloads
{
    private const string FallbackContentType = "application/octet-stream";

    private static readonly FileExtensionContentTypeProvider _contentTypes = new();

    public LocalDownload Open(string token)
    {
        bool isLocal = options.Value.StorageProvider == StorageProvider.LocalFileSystem;
        string path = isLocal ? tokens.ReadPath(token) : string.Empty;

        if (path.Length == 0 || !storage.HasFile(path))
        {
            return LocalDownload.Empty;
        }

        string fileName = DocumentBlobPath.FileNameFrom(path);
        bool isKnown = _contentTypes.TryGetContentType(fileName, out string? known);
        string contentType = isKnown ? known! : FallbackContentType;

        return new LocalDownload(storage.OpenRead(path), fileName, contentType);
    }
}
