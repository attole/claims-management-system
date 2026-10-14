using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Issues.Events;

public sealed record ValidationIssueCleared(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid ClaimId,
    Guid IssueId,
    string Code) : DomainEvent(EventId, OccurredAt, ActorId);
