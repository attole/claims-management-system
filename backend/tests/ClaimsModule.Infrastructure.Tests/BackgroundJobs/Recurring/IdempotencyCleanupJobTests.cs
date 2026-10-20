using ClaimsModule.Application.Idempotency;
using ClaimsModule.Application.Results;
using ClaimsModule.Infrastructure.BackgroundJobs.Recurring;
using MediatR;

namespace ClaimsModule.Infrastructure.Tests.BackgroundJobs.Recurring;

public sealed class IdempotencyCleanupJobTests
{
    [Fact]
    public async Task RunAsync_SendsThePurgeCommand()
    {
        var mediator = CreateMediator(Result<IdempotencyPurgeDto>.Success(new IdempotencyPurgeDto { PurgedCount = 3 }));
        var job = new IdempotencyCleanupJob(mediator.Object);

        await job.RunAsync(CancellationToken.None);

        mediator.Verify(
            mock => mock.Send(It.IsAny<PurgeExpiredIdempotencyKeysCommand>(), CancellationToken.None),
            Times.Once());
    }

    [Fact]
    public async Task RunAsync_WhenTheCommandIsRefused_Throws()
    {
        var mediator = CreateMediator(Result<IdempotencyPurgeDto>.Failure([Error.Conflict("PURGE", "refused")]));
        var job = new IdempotencyCleanupJob(mediator.Object);

        var act = () => job.RunAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*PURGE*");
    }

    private static Mock<IMediator> CreateMediator(Result<IdempotencyPurgeDto> result)
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(mock => mock.Send(It.IsAny<PurgeExpiredIdempotencyKeysCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        return mediator;
    }
}
