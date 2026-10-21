using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class ValidationIssueAcknowledgedAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ValidationIssueAcknowledgedEvent>
{
    public async Task HandleAsync(
        ValidationIssueAcknowledgedEvent domainEvent,
        ExtendedEvent extendedEvent,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.Additional.ValidationIssueAcknowledged,
            $"Validation issue {domainEvent.Code} acknowledged.")
        {
            NewValue = AuditJson.From(new { domainEvent.Code }),
            RelatedEntityId = domainEvent.IssueId,
            RelatedEntityType = nameof(ClaimValidationIssue),
        };

        await audit.WriteAsync(entry, extendedEvent, cancellationToken);
    }
}
