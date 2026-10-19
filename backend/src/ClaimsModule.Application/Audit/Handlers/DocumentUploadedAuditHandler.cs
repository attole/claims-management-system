using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class DocumentUploadedAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ClaimDocumentUploadedEvent>
{
    public async Task HandleAsync(
        ClaimDocumentUploadedEvent domainEvent,
        EventContext context,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.DocumentUploaded,
            $"Document {domainEvent.DocumentName} uploaded.")
        {
            NewValue = AuditJson.From(new { domainEvent.DocumentType, domainEvent.DocumentName }),
            RelatedEntityId = domainEvent.ClaimDocumentId,
            RelatedEntityType = nameof(ClaimDocument),
        };

        await audit.WriteAsync(entry, context, cancellationToken);
    }
}
