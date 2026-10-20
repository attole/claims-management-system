using ClaimsModule.Application.ClaimDocuments.Commands;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.BackgroundJobs.Recurring;

public sealed class BlobSweepJob(IMediator mediator, IOptions<BlobSweepOptions> options)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var command = new SweepOrphanBlobsCommand(options.Value.MinimumAge);
        var result = await mediator.Send(command, cancellationToken);

        JobResult.ThrowIfFailed(result);
    }
}
