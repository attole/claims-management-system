using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ClaimDocuments.Events;

public sealed record ClaimDocumentRemovedEvent(Guid ClaimDocumentId, Guid ClaimId) : DomainEvent;
