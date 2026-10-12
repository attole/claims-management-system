using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Tests.Support.Common;

public sealed record SampleEvent(Guid EventId, DateTimeOffset OccurredAt, Guid ActorId)
    : DomainEvent(EventId, OccurredAt, ActorId);
