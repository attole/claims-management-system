using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.ReserveComponents.History.Commands;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Results;
using ClaimsModule.Infrastructure.Tests.Support.BackgroundJobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Tests.BackgroundJobs;

[Collection(HangfireCollection.Name)]
public sealed class BackgroundJobsServiceExtensionsTests : IAsyncLifetime
{
    private HangfireTestHost _database = null!;

    public async Task InitializeAsync()
    {
        _database = await HangfireTestHost.CreateAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
    }

    [HangfireFact]
    [Trait("Rule", "S:JOB-GL")]
    public async Task EnqueueAsync_OnARealServerAndDatabase_RunsTheJobAndMarksTheRowPosted()
    {
        var organisationId = Guid.NewGuid();
        var claimId = Guid.NewGuid();
        var historyId = Guid.NewGuid();
        var received = new TaskCompletionSource<MarkReservePostedCommand>();
        var mediator = new Mock<IMediator>();
        var status = new ReserveHistoryStatusDto(historyId, "Pending", "Posted");
        var posted = Result<ReserveHistoryStatusDto>.Success(status);
        mediator
            .Setup(mock => mock.Send(
                It.IsAny<IRequest<Result<ReserveHistoryStatusDto>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<ReserveHistoryStatusDto>>, CancellationToken>(
                (command, _) => received.TrySetResult((MarkReservePostedCommand)command))
            .ReturnsAsync(posted);
        var host = _database.Build(new Dictionary<string, string?>(), mediator.Object);
        await host.StartAsync();
        await using var scope = host.Services.CreateAsyncScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<IPostingScheduler>();

        await scheduler.EnqueueAsync(organisationId, claimId, historyId, "job-1", CancellationToken.None);

        var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(30));
        result.Should().Be(new MarkReservePostedCommand(claimId, historyId, "job-1"));
    }
}
