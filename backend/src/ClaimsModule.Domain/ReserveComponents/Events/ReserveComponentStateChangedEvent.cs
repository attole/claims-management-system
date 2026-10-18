using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveComponents.Events;

public sealed record ReserveComponentStateChangedEvent(
    Guid ReserveComponentId,
    Guid ClaimId,
    string From,
    string To) : DomainEvent;
