using ClaimsModule.Application.Sla;
using Hangfire;
using MediatR;

namespace ClaimsModule.Infrastructure.BackgroundJobs.Recurring;

public sealed class SlaBreachJob(IMediator mediator)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DetectSlaBreachesCommand(), cancellationToken);

        JobResult.ThrowIfFailed(result);
    }
}
