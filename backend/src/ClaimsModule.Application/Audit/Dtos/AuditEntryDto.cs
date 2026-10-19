namespace ClaimsModule.Application.Audit.Dtos;

public sealed record AuditEntryDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public Guid ClaimId { get; init; } = Guid.Empty;

    public string EventType { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string OldValue { get; init; } = string.Empty;

    public string NewValue { get; init; } = string.Empty;

    public Guid RelatedEntityId { get; init; } = Guid.Empty;

    public string RelatedEntityType { get; init; } = string.Empty;

    public Guid CorrelationId { get; init; } = Guid.Empty;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.MinValue;

    public Guid CreatedByUserId { get; init; } = Guid.Empty;
}
