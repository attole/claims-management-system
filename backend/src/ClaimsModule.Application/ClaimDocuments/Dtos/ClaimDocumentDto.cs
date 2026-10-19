namespace ClaimsModule.Application.ClaimDocuments.Dtos;

public sealed record ClaimDocumentDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public Guid ClaimId { get; init; } = Guid.Empty;

    public string DocumentType { get; init; } = string.Empty;

    public string DocumentName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long FileSizeBytes { get; init; } = 0;

    public string Notes { get; init; } = string.Empty;

    public Guid UploadedByUserId { get; init; } = Guid.Empty;

    public DateTimeOffset UploadedAt { get; init; } = DateTimeOffset.MinValue;

    public bool IsDeleted { get; init; } = false;

    public DateTimeOffset DeletedAt { get; init; } = DateTimeOffset.MinValue;
}
