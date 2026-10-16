using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Events;

public sealed record ClaimCreatedEvent(
    Guid ClaimId,
    string ClaimNumber) : DomainEvent;
