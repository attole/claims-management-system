namespace ClaimsModule.Application.Claims;

public sealed record ClaimDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public string ClaimNumber { get; init; } = string.Empty;

    public string State { get; init; } = string.Empty;

    public string Severity { get; init; } = string.Empty;

    public DateTimeOffset ReportedDate { get; init; } = DateTimeOffset.MinValue;

    public Guid PolicyId { get; init; } = Guid.Empty;

    public Guid AssignedHandlerId { get; init; } = Guid.Empty;

    public string Notes { get; init; } = string.Empty;

    public bool HasAggregateLimitOverride { get; init; } = false;
}
