using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class ReserveApprovalAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ReserveHistoryApprovalStateChangedEvent>
{
    private static readonly IReadOnlyDictionary<string, string> _eventTypesByTarget = new Dictionary<string, string>
    {
        [nameof(ReserveApprovalStateKind.AutoApproved)] = AuditEventTypes.ReserveAutoApproved,
        [nameof(ReserveApprovalStateKind.Approved)] = AuditEventTypes.ReserveApproved,
        [nameof(ReserveApprovalStateKind.Rejected)] = AuditEventTypes.ReserveRejected,
        [nameof(ReserveApprovalStateKind.Cancelled)] = AuditEventTypes.ReserveRetracted,
    };

    public async Task HandleAsync(
        ReserveHistoryApprovalStateChangedEvent domainEvent,
        EventContext context,
        CancellationToken cancellationToken)
    {
        if (_eventTypesByTarget.TryGetValue(domainEvent.To, out var eventType))
        {
            var entry = CreateEntry(domainEvent, eventType);

            await audit.WriteAsync(entry, context, cancellationToken);
        }
    }

    private static AuditEntry CreateEntry(ReserveHistoryApprovalStateChangedEvent domainEvent, string eventType)
    {
        var description = $"Reserve approval changed from {domainEvent.From} to {domainEvent.To}.";
        bool isRejection = eventType == AuditEventTypes.ReserveRejected;
        string oldReason = isRejection ? domainEvent.Reason : string.Empty;
        string newReason = isRejection ? string.Empty : domainEvent.Reason;

        return new AuditEntry(domainEvent.ClaimId, eventType, description)
        {
            OldValue = AuditJson.From(new { Status = domainEvent.From, Reason = oldReason }),
            NewValue = AuditJson.From(new { Status = domainEvent.To, Reason = newReason }),
            RelatedEntityId = domainEvent.ReserveHistoryId,
            RelatedEntityType = nameof(ReserveHistory),
        };
    }
}
