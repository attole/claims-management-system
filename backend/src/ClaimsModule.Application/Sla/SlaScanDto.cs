namespace ClaimsModule.Application.Sla;

public sealed record SlaScanDto
{
    public int BreachCount { get; init; } = 0;
}
