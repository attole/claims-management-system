using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Application.Events.Contracts;

public sealed record EventContext(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid CorrelationId,
    DomainEvent Event) : AuditContext(OccurredAt, ActorId, CorrelationId);
