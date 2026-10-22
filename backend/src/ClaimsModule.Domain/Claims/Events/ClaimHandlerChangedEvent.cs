using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Events;

public sealed record ClaimHandlerChangedEvent(
    Guid ClaimId,
    Guid FromHandlerId,
    Guid ToHandlerId) : DomainEvent;
