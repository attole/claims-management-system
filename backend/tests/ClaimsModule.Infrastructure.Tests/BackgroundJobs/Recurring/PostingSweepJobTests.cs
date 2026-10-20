using ClaimsModule.Application.ReserveComponents.History.Commands;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Results;
using ClaimsModule.Infrastructure.BackgroundJobs.Recurring;
using MediatR;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Tests.BackgroundJobs.Recurring;

public sealed class PostingSweepJobTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public async Task RunAsync_SendsTheResendCommandWithTheConfiguredMinimumAge()
    {
        var mediator = CreateMediator(Result<ResendPostingsDto>.Success(new ResendPostingsDto { ResentCount = 1 }));
        var options = Options.Create(new PostingSweepOptions { MinimumAge = TimeSpan.FromMinutes(7) });
        var job = new PostingSweepJob(mediator.Object, options);

        await job.RunAsync(CancellationToken.None);

        mediator.Verify(
            mock => mock.Send(new ResendStalePostingsCommand(TimeSpan.FromMinutes(7)), CancellationToken.None),
            Times.Once());
    }

    [Fact]
    public async Task RunAsync_WhenTheCommandIsRefused_Throws()
    {
        var mediator = CreateMediator(Result<ResendPostingsDto>.Failure([Error.Conflict("SWEEP", "refused")]));
        var job = new PostingSweepJob(mediator.Object, Options.Create(new PostingSweepOptions()));

        var act = () => job.RunAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*SWEEP*");
    }

    private static Mock<IMediator> CreateMediator(Result<ResendPostingsDto> result)
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(mock => mock.Send(It.IsAny<ResendStalePostingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        return mediator;
    }
}
