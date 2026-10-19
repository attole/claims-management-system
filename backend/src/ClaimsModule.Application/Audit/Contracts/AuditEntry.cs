namespace ClaimsModule.Application.Audit.Contracts;

public sealed record AuditEntry(Guid ClaimId, string EventType, string Description)
{
    public string OldValue { get; init; } = string.Empty;

    public string NewValue { get; init; } = string.Empty;

    public Guid RelatedEntityId { get; init; } = Guid.Empty;

    public string RelatedEntityType { get; init; } = string.Empty;
}
