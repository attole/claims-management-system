using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.ReserveComponents.History.Commands;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ReserveComponents.History.Commands;

public sealed class ResendStalePostingsCommandTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public async Task Handle_EnqueuesEachPendingApprovedRowWithoutAJobId()
    {
        var first = new UnpostedReserve(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var second = new UnpostedReserve(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var scheduler = new Mock<IPostingScheduler>();
        var handler = CreateHandler(CreateReader([first, second]).Object, scheduler);
        var command = new ResendStalePostingsCommand(TimeSpan.FromMinutes(10));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.ResentCount.Should().Be(2);
        scheduler.Verify(
            mock => mock.EnqueueAsync(
                first.OrganisationId,
                first.ClaimId,
                first.ReserveHistoryId,
                string.Empty,
                CancellationToken.None),
            Times.Once());
        scheduler.Verify(
            mock => mock.EnqueueAsync(
                second.OrganisationId,
                second.ClaimId,
                second.ReserveHistoryId,
                string.Empty,
                CancellationToken.None),
            Times.Once());
    }

    [Fact]
    public async Task Handle_AsksForRowsApprovedBeforeTheMinimumAge()
    {
        var reader = CreateReader([]);
        var handler = CreateHandler(reader.Object, new Mock<IPostingScheduler>());
        var command = new ResendStalePostingsCommand(TimeSpan.FromMinutes(10));

        await handler.Handle(command, CancellationToken.None);

        reader.Verify(
            mock => mock.ListStalePendingPostingsAsync(
                ClaimHandlerContext.Now.AddMinutes(-10),
                CancellationToken.None),
            Times.Once());
    }

    [Fact]
    public void Validate_WithoutAMinimumAge_FailsOnTheField()
    {
        var validator = new ResendStalePostingsCommandValidator();
        var command = new ResendStalePostingsCommand(TimeSpan.Zero);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.MinimumAge);
    }

    private static Mock<IClaimReserveReader> CreateReader(IReadOnlyList<UnpostedReserve> reserves)
    {
        var reader = new Mock<IClaimReserveReader>();
        reader
            .Setup(mock => mock.ListStalePendingPostingsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(reserves);

        return reader;
    }

    private static ResendStalePostingsCommandHandler CreateHandler(
        IClaimReserveReader reader,
        Mock<IPostingScheduler> scheduler)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(mock => mock.UtcNow).Returns(ClaimHandlerContext.Now);

        return new ResendStalePostingsCommandHandler(reader, scheduler.Object, clock.Object);
    }
}
