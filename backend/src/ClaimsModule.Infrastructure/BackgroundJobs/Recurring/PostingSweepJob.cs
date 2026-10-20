using ClaimsModule.Application.ReserveComponents.History.Commands;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.BackgroundJobs.Recurring;

public sealed class PostingSweepJob(IMediator mediator, IOptions<PostingSweepOptions> options)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var command = new ResendStalePostingsCommand(options.Value.MinimumAge);
        var result = await mediator.Send(command, cancellationToken);

        JobResult.ThrowIfFailed(result);
    }
}
