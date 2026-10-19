namespace ClaimsModule.Application.Claims.Issues;

public sealed record IssueDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public Guid ClaimId { get; init; } = Guid.Empty;

    public string Code { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string Field { get; init; } = string.Empty;

    public string Severity { get; init; } = string.Empty;

    public string State { get; init; } = string.Empty;

    public DateTimeOffset ResolvedAt { get; init; } = DateTimeOffset.MinValue;

    public Guid ResolvedBy { get; init; } = Guid.Empty;
}
