namespace ClaimsModule.Application.Jobs;

public interface IPostingScheduler
{
    Task EnqueueAsync(Guid claimId, Guid reserveHistoryId, string jobId, CancellationToken cancellationToken);
}
