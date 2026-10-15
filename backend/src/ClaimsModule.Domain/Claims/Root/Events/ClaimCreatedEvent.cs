using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Root.Events;

public sealed record ClaimCreatedEvent(
    EventStamp Stamp,
    Guid ClaimId,
    string ClaimNumber) : DomainEvent(Stamp);
