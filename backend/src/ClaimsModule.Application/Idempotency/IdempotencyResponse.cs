namespace ClaimsModule.Application.Idempotency;

public sealed record IdempotencyResponse(int StatusCode, string Body)
{
    public static IdempotencyResponse Empty { get; } = new(0, string.Empty);

    public bool IsSet => StatusCode > 0;
}
