using ClaimsModule.Application.Abstractions;
using Hangfire;

namespace ClaimsModule.Infrastructure.BackgroundJobs;

internal sealed class HangfirePostingScheduler(IBackgroundJobClient client) : IPostingScheduler
{
    public Task EnqueueAsync(
        Guid organisationId,
        Guid claimId,
        Guid reserveHistoryId,
        string jobId,
        CancellationToken cancellationToken)
    {
        client.Enqueue<ReservePostingJob>(job => job.RunAsync(
            organisationId,
            claimId,
            reserveHistoryId,
            jobId,
            null!,
            CancellationToken.None));

        return Task.CompletedTask;
    }
}
