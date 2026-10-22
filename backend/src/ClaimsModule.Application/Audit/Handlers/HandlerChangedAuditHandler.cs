using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class HandlerChangedAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ClaimHandlerChangedEvent>
{
    public async Task HandleAsync(
        ClaimHandlerChangedEvent domainEvent,
        ExtendedEvent extendedEvent,
        CancellationToken cancellationToken)
    {
        bool isFirstAssignment = domainEvent.FromHandlerId == Guid.Empty;
        bool isUnchanged = domainEvent.FromHandlerId == domainEvent.ToHandlerId;

        if (isFirstAssignment || isUnchanged)
        {
            return;
        }

        var entry = new AuditEntry(domainEvent.ClaimId, AuditEventTypes.Additional.HandlerChanged, "Handler changed.")
        {
            OldValue = AuditJson.From(new { HandlerId = domainEvent.FromHandlerId }),
            NewValue = AuditJson.From(new { HandlerId = domainEvent.ToHandlerId }),
            RelatedEntityId = domainEvent.ClaimId,
            RelatedEntityType = nameof(Claim),
        };

        await audit.WriteAsync(entry, extendedEvent, cancellationToken);
    }
}
