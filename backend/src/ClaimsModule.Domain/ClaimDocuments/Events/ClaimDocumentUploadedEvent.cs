using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ClaimDocuments.Events;

public sealed record ClaimDocumentUploadedEvent(
    Guid ClaimDocumentId,
    Guid ClaimId,
    string DocumentType,
    string DocumentName) : DomainEvent;
