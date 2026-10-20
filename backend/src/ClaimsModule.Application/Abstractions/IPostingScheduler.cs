namespace ClaimsModule.Application.Abstractions;

public interface IPostingScheduler
{
    Task EnqueueAsync(
        Guid organisationId,
        Guid claimId,
        Guid reserveHistoryId,
        string jobId,
        CancellationToken cancellationToken);
}
