using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class ClaimCreatedAuditHandler(IAuditLogService audit) : IBeforeCommitEventHandler<ClaimCreatedEvent>
{
    public async Task HandleAsync(
        ClaimCreatedEvent domainEvent,
        EventContext context,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.ClaimCreated,
            $"Claim {domainEvent.ClaimNumber} created.")
        {
            NewValue = AuditJson.From(new { domainEvent.ClaimNumber }),
            RelatedEntityId = domainEvent.ClaimId,
            RelatedEntityType = nameof(Claim),
        };

        await audit.WriteAsync(entry, context, cancellationToken);
    }
}
