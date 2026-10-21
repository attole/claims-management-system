namespace ClaimsModule.Application.Idempotency;

public sealed record IdempotencyDecision(IdempotencyStatus Status)
{
    public IdempotencyResponse Stored { get; init; } = IdempotencyResponse.Empty;
}
