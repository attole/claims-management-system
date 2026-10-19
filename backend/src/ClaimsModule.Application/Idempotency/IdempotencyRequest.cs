namespace ClaimsModule.Application.Idempotency;

public sealed record IdempotencyRequest(string Key, string Method, string Route, string BodyHash);
