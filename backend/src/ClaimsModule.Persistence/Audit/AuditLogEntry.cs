using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Persistence.Audit;

internal sealed class AuditLogEntry : Entity, IAppendOnly
{
    private AuditLogEntry()
    {
    }

    public AuditLogEntry(Guid id, Guid organisationId, Guid claimId, string eventType, string description)
        : base(id)
    {
        OrganisationId = organisationId;
        ClaimId = claimId;
        EventType = eventType;
        Description = description;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public string EventType { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? OldValue { get; init; }

    public string? NewValue { get; init; }

    public Guid? RelatedEntityId { get; init; }

    public string? RelatedEntityType { get; init; }

    public Guid? CorrelationId { get; init; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid? UserCreated { get; private set; }
}
