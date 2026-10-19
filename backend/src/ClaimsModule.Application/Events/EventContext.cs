using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Application.Events;

public sealed record EventContext(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid CorrelationId,
    DomainEvent Event);
