namespace ClaimsModule.Application.CauseOfLoss;

public sealed record CauseCodeDto
{
    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string PerilCategory { get; init; } = string.Empty;
}
