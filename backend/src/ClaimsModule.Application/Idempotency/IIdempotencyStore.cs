namespace ClaimsModule.Application.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyBegin> BeginAsync(IdempotencyRequest request, CancellationToken cancellationToken);

    Task CompleteAsync(
        IdempotencyRequest request,
        IdempotencyResponse response,
        CancellationToken cancellationToken);

    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken);
}
