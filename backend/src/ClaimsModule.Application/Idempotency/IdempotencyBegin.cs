namespace ClaimsModule.Application.Idempotency;

public sealed record IdempotencyBegin(IdempotencyStatus Status)
{
    public IdempotencyResponse Stored { get; init; } = IdempotencyResponse.Empty;
}
