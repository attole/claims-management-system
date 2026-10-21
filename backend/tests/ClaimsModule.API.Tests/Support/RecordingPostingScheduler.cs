using ClaimsModule.Application.Abstractions;

namespace ClaimsModule.API.Tests.Support;

public sealed class RecordingPostingScheduler : IPostingScheduler
{
    public List<(Guid OrganisationId, Guid ClaimId, Guid ReserveHistoryId, string JobId)> Calls { get; } = [];

    public Task EnqueueAsync(
        Guid organisationId,
        Guid claimId,
        Guid reserveHistoryId,
        string jobId,
        CancellationToken cancellationToken)
    {
        Calls.Add((organisationId, claimId, reserveHistoryId, jobId));

        return Task.CompletedTask;
    }
}
