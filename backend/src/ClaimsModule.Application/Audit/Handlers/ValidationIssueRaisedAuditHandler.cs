using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Events;

namespace ClaimsModule.Application.Audit.Handlers;

public sealed class ValidationIssueRaisedAuditHandler(IAuditLogService audit)
    : IBeforeCommitEventHandler<ValidationIssueRaisedEvent>
{
    public async Task HandleAsync(
        ValidationIssueRaisedEvent domainEvent,
        EventContext context,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            domainEvent.ClaimId,
            AuditEventTypes.ValidationIssueAdded,
            $"Validation issue {domainEvent.Code} recorded.")
        {
            NewValue = AuditJson.From(new { domainEvent.Code, domainEvent.Severity }),
            RelatedEntityId = domainEvent.IssueId,
            RelatedEntityType = nameof(ClaimValidationIssue),
        };

        await audit.WriteAsync(entry, context, cancellationToken);
    }
}
