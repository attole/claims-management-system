namespace ClaimsModule.Application.ClaimDocuments.Dtos;

public sealed record ClaimDocumentLinkDto(ClaimDocumentDto Document, string DownloadUrl);
