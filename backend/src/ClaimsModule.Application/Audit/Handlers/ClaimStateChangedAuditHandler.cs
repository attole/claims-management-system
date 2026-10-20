using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class ClaimStateChangedAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ClaimStateChangedEvent>
{
    private static readonly IReadOnlyDictionary<string, string> _eventTypesByTarget = new Dictionary<string, string>
    {
        [nameof(ClaimStateKind.Closed)] = AuditEventTypes.ClaimClosed,
        [nameof(ClaimStateKind.Reopened)] = AuditEventTypes.ClaimReopened,
    };

    public async Task HandleAsync(
        ClaimStateChangedEvent domainEvent,
        ExtendedEvent extendedEvent,
        CancellationToken cancellationToken)
    {
        var changed = CreateEntry(domainEvent, AuditEventTypes.StatusChanged);

        await audit.WriteAsync(changed, extendedEvent, cancellationToken);

        if (_eventTypesByTarget.TryGetValue(domainEvent.To, out var eventType))
        {
            var specific = CreateEntry(domainEvent, eventType);

            await audit.WriteAsync(specific, extendedEvent, cancellationToken);
        }
    }

    private static AuditEntry CreateEntry(ClaimStateChangedEvent domainEvent, string eventType)
    {
        var description = $"Claim status changed from {domainEvent.From} to {domainEvent.To}.";

        return new AuditEntry(domainEvent.ClaimId, eventType, description)
        {
            OldValue = AuditJson.From(new { Status = domainEvent.From }),
            NewValue = AuditJson.From(new { Status = domainEvent.To, Reason = domainEvent.Reason }),
            RelatedEntityId = domainEvent.ClaimId,
            RelatedEntityType = nameof(Claim),
        };
    }
}
