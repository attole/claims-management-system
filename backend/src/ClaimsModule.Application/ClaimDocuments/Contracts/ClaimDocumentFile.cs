using ClaimsModule.Application.ClaimDocuments.Dtos;

namespace ClaimsModule.Application.ClaimDocuments.Contracts;

public sealed record ClaimDocumentFile(ClaimDocumentDto Document, string BlobPath);
