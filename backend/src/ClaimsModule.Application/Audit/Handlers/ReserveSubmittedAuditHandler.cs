using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class ReserveSubmittedAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ReserveHistorySubmittedEvent>
{
    public async Task HandleAsync(
        ReserveHistorySubmittedEvent domainEvent,
        EventContext context,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.ReserveCreated,
            $"Reserve of {domainEvent.Amount} submitted.")
        {
            NewValue = AuditJson.From(new { domainEvent.Amount }),
            RelatedEntityId = domainEvent.ReserveHistoryId,
            RelatedEntityType = nameof(ReserveHistory),
        };

        await audit.WriteAsync(entry, context, cancellationToken);
    }
}
