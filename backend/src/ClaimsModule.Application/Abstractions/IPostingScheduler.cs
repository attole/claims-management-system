namespace ClaimsModule.Application.Abstractions;

public interface IPostingScheduler
{
    Task EnqueueAsync(Guid claimId, Guid reserveHistoryId, string jobId, CancellationToken cancellationToken);
}
