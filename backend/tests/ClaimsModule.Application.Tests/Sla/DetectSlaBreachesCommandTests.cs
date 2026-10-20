using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Sla;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.Events;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Sla;

public sealed class DetectSlaBreachesCommandTests
{
    private static readonly Guid _correlationId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    [Fact]
    [Trait("Rule", "S:JOB-SLA")]
    public async Task Handle_WritesAnSlaBreachEntryForEachBreachedClaim()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var log = new RecordingAuditLog();
        var handler = CreateHandler(CreateReader([first, second]), log, new Mock<IUnitOfWork>());

        var result = await handler.Handle(new DetectSlaBreachesCommand(), CancellationToken.None);

        result.Value.BreachCount.Should().Be(2);
        log.Entries.Select(entry => entry.ClaimId).Should().Equal(first, second);
        log.Entries.Should().OnlyContain(entry => entry.EventType == AuditEventTypes.SlaBreachDetected);
    }

    [Fact]
    [Trait("Rule", "S:JOB-SLA")]
    [Trait("Rule", "S:JOB-SLA-Dedupe")]
    public async Task Handle_AsksForClaimsStaleFor48HoursAndNotFlaggedWithin24Hours()
    {
        var reader = new Mock<ISlaClaimReader>();
        reader
            .Setup(mock => mock.ListBreachedClaimIdsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = CreateHandler(reader.Object, new RecordingAuditLog(), new Mock<IUnitOfWork>());
        var now = ClaimHandlerContext.Now;

        await handler.Handle(new DetectSlaBreachesCommand(), CancellationToken.None);

        reader.Verify(
            mock => mock.ListBreachedClaimIdsAsync(now.AddHours(-48), now.AddHours(-24), CancellationToken.None),
            Times.Once());
    }

    [Fact]
    [Trait("Rule", "S:AUD-Correlation")]
    public async Task Handle_WritesTheEntriesAsTheSystemActorWithTheCorrelationId()
    {
        var log = new RecordingAuditLog();
        var handler = CreateHandler(CreateReader([Guid.NewGuid()]), log, new Mock<IUnitOfWork>());

        await handler.Handle(new DetectSlaBreachesCommand(), CancellationToken.None);

        var context = log.Contexts.Should().ContainSingle().Subject;
        context.ActorId.Should().Be(Guid.Empty);
        context.OccurredAt.Should().Be(ClaimHandlerContext.Now);
        context.CorrelationId.Should().Be(_correlationId);
    }

    [Fact]
    public async Task Handle_WithBreachedClaims_Commits()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = CreateHandler(CreateReader([Guid.NewGuid()]), new RecordingAuditLog(), unitOfWork);

        await handler.Handle(new DetectSlaBreachesCommand(), CancellationToken.None);

        unitOfWork.Verify(mock => mock.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Handle_WithoutBreachedClaims_WritesNothing()
    {
        var log = new RecordingAuditLog();
        var handler = CreateHandler(CreateReader([]), log, new Mock<IUnitOfWork>());

        var result = await handler.Handle(new DetectSlaBreachesCommand(), CancellationToken.None);

        result.Value.BreachCount.Should().Be(0);
        log.Entries.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ForTheCommand_HasNoErrors()
    {
        var validator = new DetectSlaBreachesCommandValidator();

        var result = validator.TestValidate(new DetectSlaBreachesCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static ISlaClaimReader CreateReader(IReadOnlyList<Guid> claimIds)
    {
        var reader = new Mock<ISlaClaimReader>();
        reader
            .Setup(mock => mock.ListBreachedClaimIdsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimIds);

        return reader.Object;
    }

    private static DetectSlaBreachesCommandHandler CreateHandler(
        ISlaClaimReader reader,
        RecordingAuditLog log,
        Mock<IUnitOfWork> unitOfWork)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(mock => mock.UtcNow).Returns(ClaimHandlerContext.Now);
        var correlation = new Mock<ICorrelationContext>();
        correlation.SetupGet(mock => mock.CorrelationId).Returns(_correlationId);

        return new DetectSlaBreachesCommandHandler(
            reader,
            log,
            unitOfWork.Object,
            clock.Object,
            correlation.Object);
    }
}
