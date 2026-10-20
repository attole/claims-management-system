using ClaimsModule.Application.Idempotency;
using Hangfire;
using MediatR;

namespace ClaimsModule.Infrastructure.BackgroundJobs.Recurring;

public sealed class IdempotencyCleanupJob(IMediator mediator)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new PurgeExpiredIdempotencyKeysCommand(), cancellationToken);

        JobResult.ThrowIfFailed(result);
    }
}
