using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class PartyRemovedAuditHandler(IAuditLogService audit) : IBeforeCommitEventHandler<PartyRemovedEvent>
{
    public async Task HandleAsync(
        PartyRemovedEvent domainEvent,
        EventContext context,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.PartyRemoved,
            $"Party with role {domainEvent.Role} removed.")
        {
            OldValue = AuditJson.From(new { domainEvent.Role }),
            RelatedEntityId = domainEvent.PartyId,
            RelatedEntityType = nameof(ClaimParty),
        };

        await audit.WriteAsync(entry, context, cancellationToken);
    }
}
