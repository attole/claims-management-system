using ClaimsModule.Application.ReserveComponents.History.Commands;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Results;
using ClaimsModule.Infrastructure.BackgroundJobs;
using ClaimsModule.Infrastructure.Identity;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Tests.Support.BackgroundJobs;

internal sealed class ReservePostingJobHarness
{
    public const string HangfireJobId = "hangfire-7";

    public static readonly Guid OrganisationId = Guid.NewGuid();
    public static readonly Guid ClaimId = Guid.NewGuid();
    public static readonly Guid HistoryId = Guid.NewGuid();

    public ReservePostingJobHarness()
    {
        var services = new ServiceCollection();
        services.AddScoped<ScopedCurrentUser>();
        services.AddScoped<IMediator>(provider =>
        {
            ObservedOrganisations.Add(provider.GetRequiredService<ScopedCurrentUser>().OrganisationId);

            return Mediator.Object;
        });
        Subject = new ReservePostingJob(services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());
    }

    public Mock<IMediator> Mediator { get; } = new();

    public List<Guid> ObservedOrganisations { get; } = [];

    public ReservePostingJob Subject { get; }

    public static ReserveHistoryStatusDto Status()
    {
        return new ReserveHistoryStatusDto(HistoryId, "Pending", "Posted");
    }

    public static PerformContext Context(int retryCount)
    {
        var connection = new Mock<IStorageConnection>();
        connection
            .Setup(mock => mock.GetJobParameter(HangfireJobId, "RetryCount"))
            .Returns(retryCount.ToString());
        var job = Job.FromExpression<ReservePostingJob>(posting =>
            posting.RunAsync(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty, null!, CancellationToken.None));
        var backgroundJob = new BackgroundJob(HangfireJobId, job, DateTime.UtcNow);

        return new PerformContext(
            Mock.Of<JobStorage>(),
            connection.Object,
            backgroundJob,
            Mock.Of<IJobCancellationToken>());
    }

    public Task RunAsync(string jobId, int retryCount)
    {
        return Subject.RunAsync(OrganisationId, ClaimId, HistoryId, jobId, Context(retryCount), CancellationToken.None);
    }

    public void SetupPosted(Result<ReserveHistoryStatusDto> result)
    {
        Mediator
            .Setup(mock => mock.Send(It.IsAny<MarkReservePostedCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    public void SetupPostedThrows(Exception exception)
    {
        Mediator
            .Setup(mock => mock.Send(It.IsAny<MarkReservePostedCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    public void SetupFailed(Result<ReserveHistoryStatusDto> result)
    {
        Mediator
            .Setup(mock => mock.Send(It.IsAny<MarkReservePostingFailedCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }
}
