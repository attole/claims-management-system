using ClaimsModule.Application.ReserveComponents.History.Commands;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Infrastructure.Identity;
using Hangfire;
using Hangfire.Server;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.BackgroundJobs;

public sealed class ReservePostingJob(IServiceScopeFactory scopeFactory)
{
    public const int MaxRetries = 3;

    private const string FailureReason = "Posting failed after all retries";

    [AutomaticRetry(Attempts = MaxRetries, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task RunAsync(
        Guid organisationId,
        Guid claimId,
        Guid reserveHistoryId,
        string jobId,
        PerformContext context,
        CancellationToken cancellationToken)
    {
        string recordedJobId = string.IsNullOrEmpty(jobId) ? context.BackgroundJob.Id : jobId;
        bool isLastAttempt = context.GetJobParameter<int>("RetryCount") >= MaxRetries;

        try
        {
            var posted = new MarkReservePostedCommand(claimId, reserveHistoryId, recordedJobId);
            JobResult.ThrowIfFailed(await SendInNewScopeAsync(organisationId, posted, cancellationToken));
        }
        catch (Exception exception) when (isLastAttempt && exception is not OperationCanceledException)
        {
            var failed = new MarkReservePostingFailedCommand(claimId, reserveHistoryId, recordedJobId, FailureReason);
            JobResult.ThrowIfFailed(await SendInNewScopeAsync(organisationId, failed, cancellationToken));
        }
    }

    private async Task<Result<ReserveHistoryStatusDto>> SendInNewScopeAsync(
        Guid organisationId,
        IRequest<Result<ReserveHistoryStatusDto>> command,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var user = scope.ServiceProvider.GetRequiredService<ScopedCurrentUser>();
        user.Set(Guid.Empty, organisationId, UserRole.From(UserRoleKind.Handler));

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        return await mediator.Send(command, cancellationToken);
    }
}
