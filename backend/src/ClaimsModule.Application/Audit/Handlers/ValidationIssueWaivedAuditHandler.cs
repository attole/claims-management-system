using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class ValidationIssueWaivedAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ValidationIssueWaivedEvent>
{
    public async Task HandleAsync(
        ValidationIssueWaivedEvent domainEvent,
        ExtendedEvent extendedEvent,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.Additional.ValidationIssueWaived,
            $"Validation issue {domainEvent.Code} waived.")
        {
            NewValue = AuditJson.From(new { domainEvent.Code }),
            RelatedEntityId = domainEvent.IssueId,
            RelatedEntityType = nameof(ClaimValidationIssue),
        };

        await audit.WriteAsync(entry, extendedEvent, cancellationToken);
    }
}
