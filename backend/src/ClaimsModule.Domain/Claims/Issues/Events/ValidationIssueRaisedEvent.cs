using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Issues.Events;

public sealed record ValidationIssueRaisedEvent(
    EventStamp Stamp,
    Guid ClaimId,
    Guid IssueId,
    string Code,
    string Severity) : DomainEvent(Stamp);
