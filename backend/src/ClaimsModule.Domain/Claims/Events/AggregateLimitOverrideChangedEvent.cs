using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Events;

public sealed record AggregateLimitOverrideChangedEvent(
    Guid ClaimId,
    bool WasAggregateLimitOverride,
    bool HasAggregateLimitOverride) : DomainEvent;
