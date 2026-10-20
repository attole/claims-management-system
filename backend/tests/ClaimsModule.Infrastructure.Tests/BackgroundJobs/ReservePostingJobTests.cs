using ClaimsModule.Application.ReserveComponents.History.Commands;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Results;
using ClaimsModule.Infrastructure.BackgroundJobs;
using ClaimsModule.Infrastructure.Tests.Support.BackgroundJobs;

namespace ClaimsModule.Infrastructure.Tests.BackgroundJobs;

public sealed class ReservePostingJobTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public async Task RunAsync_Succeeds_MarksTheRowPostedUnderTheRecordedJobId()
    {
        var harness = new ReservePostingJobHarness();
        harness.SetupPosted(Result<ReserveHistoryStatusDto>.Success(ReservePostingJobHarness.Status()));

        await harness.RunAsync("job-1", 0);

        var expected = new MarkReservePostedCommand(
            ReservePostingJobHarness.ClaimId,
            ReservePostingJobHarness.HistoryId,
            "job-1");
        harness.Mediator.Verify(mock => mock.Send(expected, CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task RunAsync_WithoutARecordedJobId_UsesTheIdOfTheHangfireJob()
    {
        var harness = new ReservePostingJobHarness();
        harness.SetupPosted(Result<ReserveHistoryStatusDto>.Success(ReservePostingJobHarness.Status()));

        await harness.RunAsync(string.Empty, 0);

        harness.Mediator.Verify(
            mock => mock.Send(
                It.Is<MarkReservePostedCommand>(command => command.JobId == ReservePostingJobHarness.HangfireJobId),
                CancellationToken.None),
            Times.Once());
    }

    [Fact]
    public async Task RunAsync_RunsUnderTheOrganisationOfTheRow()
    {
        var harness = new ReservePostingJobHarness();
        harness.SetupPosted(Result<ReserveHistoryStatusDto>.Success(ReservePostingJobHarness.Status()));

        await harness.RunAsync("job-1", 0);

        harness.ObservedOrganisations.Should().Equal(ReservePostingJobHarness.OrganisationId);
    }

    [Fact]
    public async Task RunAsync_WhenTheCommandIsRefusedBeforeTheLastAttempt_Throws()
    {
        var harness = new ReservePostingJobHarness();
        harness.SetupPosted(Result<ReserveHistoryStatusDto>.Failure([Error.NotFound("RESERVE_NOT_FOUND", "missing")]));

        var act = () => harness.RunAsync("job-1", 0);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*RESERVE_NOT_FOUND*");
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public async Task RunAsync_WhenTheLastAttemptFails_MarksThePostingFailedInANewScope()
    {
        var harness = new ReservePostingJobHarness();
        harness.SetupPostedThrows(new TimeoutException());
        harness.SetupFailed(Result<ReserveHistoryStatusDto>.Success(ReservePostingJobHarness.Status()));

        await harness.RunAsync("job-1", ReservePostingJob.MaxRetries);

        harness.Mediator.Verify(
            mock => mock.Send(
                It.Is<MarkReservePostingFailedCommand>(command => command.JobId == "job-1"),
                CancellationToken.None),
            Times.Once());
        harness.ObservedOrganisations.Should().HaveCount(2);
    }

    [Fact]
    public async Task RunAsync_WhenTheFailedMarkIsRefused_Throws()
    {
        var harness = new ReservePostingJobHarness();
        harness.SetupPostedThrows(new TimeoutException());
        harness.SetupFailed(Result<ReserveHistoryStatusDto>.Failure([Error.Conflict("POSTING_STATE", "wrong state")]));

        var act = () => harness.RunAsync("job-1", ReservePostingJob.MaxRetries);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*POSTING_STATE*");
    }

    [Fact]
    public async Task RunAsync_WhenAnEarlierAttemptFails_DoesNotMarkThePostingFailed()
    {
        var harness = new ReservePostingJobHarness();
        harness.SetupPostedThrows(new TimeoutException());

        var act = () => harness.RunAsync("job-1", 1);

        await act.Should().ThrowAsync<TimeoutException>();
        harness.Mediator.Verify(
            mock => mock.Send(It.IsAny<MarkReservePostingFailedCommand>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [Fact]
    public async Task RunAsync_WhenTheLastAttemptIsCancelled_RethrowsWithoutMarkingFailed()
    {
        var harness = new ReservePostingJobHarness();
        harness.SetupPostedThrows(new OperationCanceledException());

        var act = () => harness.RunAsync("job-1", ReservePostingJob.MaxRetries);

        await act.Should().ThrowAsync<OperationCanceledException>();
        harness.Mediator.Verify(
            mock => mock.Send(It.IsAny<MarkReservePostingFailedCommand>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }
}
