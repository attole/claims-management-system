using ClaimsModule.Application.Results;
using ClaimsModule.Application.Sla;
using ClaimsModule.Infrastructure.BackgroundJobs.Recurring;
using MediatR;

namespace ClaimsModule.Infrastructure.Tests.BackgroundJobs.Recurring;

public sealed class SlaBreachJobTests
{
    [Fact]
    [Trait("Rule", "S:JOB-SLA")]
    public async Task RunAsync_SendsTheDetectionCommand()
    {
        var mediator = CreateMediator(Result<SlaScanDto>.Success(new SlaScanDto { BreachCount = 2 }));
        var job = new SlaBreachJob(mediator.Object);

        await job.RunAsync(CancellationToken.None);

        mediator.Verify(
            mock => mock.Send(It.IsAny<DetectSlaBreachesCommand>(), CancellationToken.None),
            Times.Once());
    }

    [Fact]
    public async Task RunAsync_WhenTheCommandIsRefused_Throws()
    {
        var mediator = CreateMediator(Result<SlaScanDto>.Failure([Error.Conflict("SLA_SCAN", "refused")]));
        var job = new SlaBreachJob(mediator.Object);

        var act = () => job.RunAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*SLA_SCAN*");
    }

    private static Mock<IMediator> CreateMediator(Result<SlaScanDto> result)
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(mock => mock.Send(It.IsAny<DetectSlaBreachesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        return mediator;
    }
}
