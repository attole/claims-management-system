namespace ClaimsModule.Domain.ClaimDocuments;

public sealed record ClaimDocumentDetails(
    string DocumentType,
    string DocumentName,
    string BlobPath,
    string ContentType,
    long FileSizeBytes)
{
    public string Notes { get; init; } = string.Empty;
}
