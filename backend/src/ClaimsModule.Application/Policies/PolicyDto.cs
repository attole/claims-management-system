namespace ClaimsModule.Application.Policies;

public sealed record PolicyDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public string PolicyNumber { get; init; } = string.Empty;

    public string ClientName { get; init; } = string.Empty;

    public DateOnly EffectiveDate { get; init; } = DateOnly.MinValue;

    public DateOnly ExpirationDate { get; init; } = DateOnly.MinValue;

    public string Status { get; init; } = string.Empty;

    public IReadOnlyList<string> CoverageTypes { get; init; } = [];
}
