using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.ClaimDocuments.Dtos;

namespace ClaimsModule.Application.ClaimDocuments;

public static class ClaimDocumentLinks
{
    private static readonly TimeSpan _downloadLifetime = TimeSpan.FromHours(1);

    public static async Task<IReadOnlyList<ClaimDocumentLinkDto>> BuildAsync(
        IStorageService storage,
        IReadOnlyList<ClaimDocumentFile> files,
        CancellationToken cancellationToken)
    {
        var links = new List<ClaimDocumentLinkDto>();

        foreach (var file in files)
        {
            var url = await storage.GetDownloadUrlAsync(file.BlobPath, _downloadLifetime, cancellationToken);

            links.Add(new ClaimDocumentLinkDto(file.Document, url));
        }

        return links;
    }
}
