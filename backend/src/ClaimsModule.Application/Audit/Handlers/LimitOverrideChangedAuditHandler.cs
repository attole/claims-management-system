using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class LimitOverrideChangedAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<AggregateLimitOverrideChangedEvent>
{
    public async Task HandleAsync(
        AggregateLimitOverrideChangedEvent domainEvent,
        ExtendedEvent extendedEvent,
        CancellationToken cancellationToken)
    {
        if (domainEvent.WasAggregateLimitOverride == domainEvent.HasAggregateLimitOverride)
        {
            return;
        }

        string state = domainEvent.HasAggregateLimitOverride ? "set" : "cleared";
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.Additional.LimitOverrideChanged,
            $"Claim limit override {state}.")
        {
            OldValue = AuditJson.From(new { HasAggregateLimitOverride = domainEvent.WasAggregateLimitOverride }),
            NewValue = AuditJson.From(new { domainEvent.HasAggregateLimitOverride }),
            RelatedEntityId = domainEvent.ClaimId,
            RelatedEntityType = nameof(Claim),
        };

        await audit.WriteAsync(entry, extendedEvent, cancellationToken);
    }
}
