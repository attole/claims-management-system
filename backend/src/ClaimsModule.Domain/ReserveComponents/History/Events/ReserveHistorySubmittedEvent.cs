using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveComponents.History.Events;

public sealed record ReserveHistorySubmittedEvent(
    Guid ReserveHistoryId,
    Guid ReserveComponentId,
    Guid ClaimId,
    decimal Amount) : DomainEvent;
