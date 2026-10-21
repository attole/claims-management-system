using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class DocumentRemovedAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ClaimDocumentRemovedEvent>
{
    public async Task HandleAsync(
        ClaimDocumentRemovedEvent domainEvent,
        ExtendedEvent extendedEvent,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.Additional.DocumentRemoved,
            "Document removed.")
        {
            RelatedEntityId = domainEvent.ClaimDocumentId,
            RelatedEntityType = nameof(ClaimDocument),
        };

        await audit.WriteAsync(entry, extendedEvent, cancellationToken);
    }
}
