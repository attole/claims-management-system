namespace ClaimsModule.Application.Idempotency;

public enum IdempotencyStatus
{
    Started,
    InProgress,
    Completed,
    BodyMismatch,
}
