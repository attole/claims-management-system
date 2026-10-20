using ClaimsModule.Application.ClaimDocuments.Commands;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.Results;
using ClaimsModule.Infrastructure.BackgroundJobs.Recurring;
using MediatR;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Tests.BackgroundJobs.Recurring;

public sealed class BlobSweepJobTests
{
    [Fact]
    public async Task RunAsync_SendsTheSweepCommandWithTheConfiguredMinimumAge()
    {
        var mediator = CreateMediator(Result<BlobSweepDto>.Success(new BlobSweepDto { DeletedCount = 1 }));
        var options = Options.Create(new BlobSweepOptions { MinimumAge = TimeSpan.FromHours(6) });
        var job = new BlobSweepJob(mediator.Object, options);

        await job.RunAsync(CancellationToken.None);

        mediator.Verify(
            mock => mock.Send(new SweepOrphanBlobsCommand(TimeSpan.FromHours(6)), CancellationToken.None),
            Times.Once());
    }

    [Fact]
    public async Task RunAsync_WhenTheCommandIsRefused_Throws()
    {
        var mediator = CreateMediator(Result<BlobSweepDto>.Failure([Error.Conflict("BLOB_SWEEP", "refused")]));
        var job = new BlobSweepJob(mediator.Object, Options.Create(new BlobSweepOptions()));

        var act = () => job.RunAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*BLOB_SWEEP*");
    }

    private static Mock<IMediator> CreateMediator(Result<BlobSweepDto> result)
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(mock => mock.Send(It.IsAny<SweepOrphanBlobsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        return mediator;
    }
}
