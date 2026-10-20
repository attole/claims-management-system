using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Idempotency;
using ClaimsModule.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Idempotency;

internal sealed class IdempotencyStore(ClaimsDbContext context, IClock clock, ICurrentUser currentUser)
    : IIdempotencyStore
{
    private static readonly TimeSpan _expiresAfter = TimeSpan.FromHours(24);

    public async Task<IdempotencyBegin> BeginAsync(IdempotencyRequest request, CancellationToken cancellationToken)
    {
        var existing = await FindAsync(request, cancellationToken);

        if (existing is null)
        {
            var created = new IdempotencyRecord(
                Ids.Next(),
                currentUser.OrganisationId,
                currentUser.Id,
                request,
                clock.UtcNow);
            context.IdempotencyRecords.Add(created);

            return new IdempotencyBegin(IdempotencyStatus.Started);
        }

        return Resolve(existing, request);
    }

    public async Task CompleteAsync(
        IdempotencyRequest request,
        IdempotencyResponse response,
        CancellationToken cancellationToken)
    {
        var record = await FindAsync(request, cancellationToken)
            ?? throw new InvalidOperationException("The idempotency key was not begun.");

        record.Complete(response);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<IdempotencyRecord?> FindAsync(IdempotencyRequest request, CancellationToken cancellationToken)
    {
        var organisationId = currentUser.OrganisationId;
        var userId = currentUser.Id;
        var staged = context.IdempotencyRecords.Local.FirstOrDefault(record =>
            record.OrganisationId == organisationId
            && record.UserId == userId
            && record.Key == request.Key
            && record.Method == request.Method
            && record.Route == request.Route);

        if (staged is not null)
        {
            return staged;
        }

        return await context.IdempotencyRecords
            .Where(record => record.Key == request.Key && record.UserId == userId)
            .Where(record => record.Method == request.Method && record.Route == request.Route)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IdempotencyBegin Resolve(IdempotencyRecord record, IdempotencyRequest request)
    {
        var now = clock.UtcNow;

        if (record.CreatedAt < now - _expiresAfter)
        {
            record.Restart(request.BodyHash, now);

            return new IdempotencyBegin(IdempotencyStatus.Started);
        }

        if (record.BodyHash != request.BodyHash)
        {
            return new IdempotencyBegin(IdempotencyStatus.BodyMismatch);
        }

        if (record.IsInProgress)
        {
            return new IdempotencyBegin(IdempotencyStatus.InProgress);
        }

        return new IdempotencyBegin(IdempotencyStatus.Completed) { Stored = record.Response };
    }
}
