using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Issues.Events;

public sealed record ValidationIssueAcknowledgedEvent(
    Guid ClaimId,
    Guid IssueId,
    string Code) : DomainEvent;
