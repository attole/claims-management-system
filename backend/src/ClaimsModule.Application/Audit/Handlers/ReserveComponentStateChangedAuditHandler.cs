using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class ReserveComponentStateChangedAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ReserveComponentStateChangedEvent>
{
    public async Task HandleAsync(
        ReserveComponentStateChangedEvent domainEvent,
        ExtendedEvent extendedEvent,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.Additional.ReserveComponentStateChanged,
            $"Reserve component changed from {domainEvent.From} to {domainEvent.To}.")
        {
            OldValue = AuditJson.From(new { Status = domainEvent.From }),
            NewValue = AuditJson.From(new { Status = domainEvent.To }),
            RelatedEntityId = domainEvent.ReserveComponentId,
            RelatedEntityType = nameof(ReserveComponent),
        };

        await audit.WriteAsync(entry, extendedEvent, cancellationToken);
    }
}
