namespace ClaimsModule.Application.Idempotency;

public sealed record IdempotencyPurgeDto
{
    public int PurgedCount { get; init; } = 0;
}
