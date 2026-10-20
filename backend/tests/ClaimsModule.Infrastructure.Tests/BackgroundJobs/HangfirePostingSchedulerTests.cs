using ClaimsModule.Infrastructure.BackgroundJobs;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace ClaimsModule.Infrastructure.Tests.BackgroundJobs;

public sealed class HangfirePostingSchedulerTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public async Task EnqueueAsync_CreatesAPostingJobCarryingTheRowAndItsOrganisation()
    {
        var organisationId = Guid.NewGuid();
        var claimId = Guid.NewGuid();
        var historyId = Guid.NewGuid();
        Job? created = null;
        var client = new Mock<IBackgroundJobClient>();
        client
            .Setup(mock => mock.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Callback<Job, IState>((job, _) => created = job)
            .Returns("1");
        var scheduler = new HangfirePostingScheduler(client.Object);

        await scheduler.EnqueueAsync(organisationId, claimId, historyId, "retry-token", CancellationToken.None);

        created!.Type.Should().Be(typeof(ReservePostingJob));
        created.Args.Take(4).Should().Equal(organisationId, claimId, historyId, "retry-token");
    }
}
