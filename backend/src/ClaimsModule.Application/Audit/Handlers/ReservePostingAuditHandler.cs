using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class ReservePostingAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ReserveHistoryPostingStateChangedEvent>
{
    private const string DebitAccount = "Change in Outstanding Reserves";

    private const string CreditAccount = "Outstanding Loss Reserves";

    private static readonly IReadOnlyDictionary<string, string> _eventTypesByTarget = new Dictionary<string, string>
    {
        [nameof(ReservePostingStateKind.Posted)] = AuditEventTypes.GlPostingSimulated,
        [nameof(ReservePostingStateKind.Failed)] = AuditEventTypes.GlPostingFailed,
    };

    public async Task HandleAsync(
        ReserveHistoryPostingStateChangedEvent domainEvent,
        ExtendedEvent extendedEvent,
        CancellationToken cancellationToken)
    {
        if (_eventTypesByTarget.TryGetValue(domainEvent.To, out var eventType))
        {
            var entry = CreateEntry(domainEvent, eventType);

            await audit.WriteAsync(entry, extendedEvent, cancellationToken);
        }
    }

    private static AuditEntry CreateEntry(ReserveHistoryPostingStateChangedEvent domainEvent, string eventType)
    {
        bool isPosted = string.Equals(eventType, AuditEventTypes.GlPostingSimulated, StringComparison.Ordinal);
        var description = isPosted
            ? FormattableString.Invariant(
                $"Simulated GL journal: DR {DebitAccount} / CR {CreditAccount}, {domainEvent.Amount:0.00}.")
            : $"Reserve posting failed: {domainEvent.FailureReason}";

        return new AuditEntry(domainEvent.ClaimId, eventType, description)
        {
            OldValue = AuditJson.From(new { Status = domainEvent.From }),
            NewValue = AuditJson.From(new
            {
                Status = domainEvent.To,
                domainEvent.JobId,
                domainEvent.Amount,
                domainEvent.ReserveComponentId,
                domainEvent.FailureReason,
                Debit = isPosted ? DebitAccount : string.Empty,
                Credit = isPosted ? CreditAccount : string.Empty,
            }),
            RelatedEntityId = domainEvent.ReserveHistoryId,
            RelatedEntityType = nameof(ReserveHistory),
        };
    }
}
