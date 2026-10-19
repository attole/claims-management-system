using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class PartyAddedAuditHandler(IAuditLogService audit) : IBeforeCommitEventHandler<PartyAddedEvent>
{
    public async Task HandleAsync(
        PartyAddedEvent domainEvent,
        EventContext context,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.PartyAdded,
            $"Party added with role {domainEvent.Role}.")
        {
            NewValue = AuditJson.From(new { domainEvent.Role }),
            RelatedEntityId = domainEvent.PartyId,
            RelatedEntityType = nameof(ClaimParty),
        };

        await audit.WriteAsync(entry, context, cancellationToken);
    }
}
