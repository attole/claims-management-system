using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using ClaimsModule.Persistence.Tests.Support.ReserveComponents;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests;

public sealed class UnitOfWorkTests
{
    private readonly Guid _correlationId = Guid.NewGuid();

    [Fact]
    public async Task CommitAsync_NewClaim_SavesIt()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var unitOfWork = CreateUnitOfWork(database, context, new Mock<IEventDispatcher>());
        var claim = TestClaims.Create(database);
        context.Claims.Add(claim);

        await unitOfWork.CommitAsync(CancellationToken.None);

        using var reader = database.CreateContext();
        reader.Claims.Should().ContainSingle(saved => saved.Id == claim.Id);
    }

    [Fact]
    public async Task CommitAsync_Events_RunBeforeTheSaveAndAfterIt()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var steps = new List<string>();
        var dispatcher = new Mock<IEventDispatcher>();
        dispatcher
            .Setup(source => source.DispatchBeforeCommitAsync(It.IsAny<IReadOnlyList<ExtendedEvent>>(), default))
            .Callback(() => steps.Add($"before:{CountSaved(database)}"))
            .Returns(Task.CompletedTask);
        dispatcher
            .Setup(source => source.DispatchAfterCommitAsync(It.IsAny<IReadOnlyList<ExtendedEvent>>(), default))
            .Callback(() => steps.Add($"after:{CountSaved(database)}"))
            .Returns(Task.CompletedTask);
        var unitOfWork = CreateUnitOfWork(database, context, dispatcher);
        context.Claims.Add(TestClaims.Create(database));

        await unitOfWork.CommitAsync(CancellationToken.None);

        steps.Should().Equal("before:0", "after:1");
    }

    [Fact]
    public async Task CommitAsync_BeforeHandlerRaisesMoreEvents_RunsAgainUntilNoneAreLeft()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var eventCounts = new List<int>();
        var dispatcher = new Mock<IEventDispatcher>();
        dispatcher
            .Setup(source => source.DispatchBeforeCommitAsync(It.IsAny<IReadOnlyList<ExtendedEvent>>(), default))
            .Callback<IReadOnlyList<ExtendedEvent>, CancellationToken>((extendedEvents, _) =>
            {
                eventCounts.Add(extendedEvents.Count);

                if (eventCounts.Count == 1)
                {
                    context.Claims.Add(TestClaims.Create(database, 2));
                }
            })
            .Returns(Task.CompletedTask);
        var unitOfWork = CreateUnitOfWork(database, context, dispatcher);
        context.Claims.Add(TestClaims.Create(database));

        await unitOfWork.CommitAsync(CancellationToken.None);

        eventCounts.Should().HaveCount(2);
        eventCounts.Should().OnlyContain(size => size > 0);
    }

    [Fact]
    public async Task CommitAsync_BeforeHandlersKeepRaisingEvents_StopsWithAnError()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var created = 1;
        var dispatcher = new Mock<IEventDispatcher>();
        dispatcher
            .Setup(source => source.DispatchBeforeCommitAsync(It.IsAny<IReadOnlyList<ExtendedEvent>>(), default))
            .Callback(() => context.Claims.Add(TestClaims.Create(database, ++created)))
            .Returns(Task.CompletedTask);
        var unitOfWork = CreateUnitOfWork(database, context, dispatcher);
        context.Claims.Add(TestClaims.Create(database));

        Func<Task> act = () => unitOfWork.CommitAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        CountSaved(database).Should().Be(0);
    }

    [Fact]
    [Trait("Rule", "S:AUD-Correlation")]
    public async Task CommitAsync_Events_AreWrappedWithTheCommandContext()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var captured = new List<ExtendedEvent>();
        var dispatcher = new Mock<IEventDispatcher>();
        dispatcher
            .Setup(source => source.DispatchAfterCommitAsync(It.IsAny<IReadOnlyList<ExtendedEvent>>(), default))
            .Callback<IReadOnlyList<ExtendedEvent>, CancellationToken>((events, _) => captured.AddRange(events))
            .Returns(Task.CompletedTask);
        var unitOfWork = CreateUnitOfWork(database, context, dispatcher);
        context.Claims.Add(TestClaims.Create(database));

        await unitOfWork.CommitAsync(CancellationToken.None);

        captured.Should().NotBeEmpty();
        captured.Should().OnlyContain(item => item.ActorId == TestDatabase.UserId
            && item.CorrelationId == _correlationId
            && item.OccurredAt == database.Now
            && item.EventId != Guid.Empty);
    }

    [Fact]
    [Trait("Rule", "S:CLAIMNO-Atomic")]
    public async Task CommitAsync_OpenTransaction_IsCommittedWithTheSave()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var unitOfWork = CreateUnitOfWork(database, context, new Mock<IEventDispatcher>());
        await context.Database.BeginTransactionAsync();
        context.Claims.Add(TestClaims.Create(database));

        await unitOfWork.CommitAsync(CancellationToken.None);

        context.Database.CurrentTransaction.Should().BeNull();
        CountSaved(database).Should().Be(1);
    }

    [Fact]
    [Trait("Rule", "S:CLAIMNO-Atomic")]
    public async Task CommitAsync_SaveFails_LeavesTheTransactionUncommitted()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var unitOfWork = CreateUnitOfWork(database, context, new Mock<IEventDispatcher>());
        await context.Database.BeginTransactionAsync();
        context.ReserveComponents.Add(
            TestReserves.Open(Ids.Next(), TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity));

        Func<Task> act = () => unitOfWork.CommitAsync(CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateException>();
        context.Database.CurrentTransaction.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitAsync_BeforeHandlerThrows_SavesNothing()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var dispatcher = new Mock<IEventDispatcher>();
        dispatcher
            .Setup(source => source.DispatchBeforeCommitAsync(It.IsAny<IReadOnlyList<ExtendedEvent>>(), default))
            .ThrowsAsync(new InvalidOperationException());
        var unitOfWork = CreateUnitOfWork(database, context, dispatcher);
        context.Claims.Add(TestClaims.Create(database));

        Func<Task> act = () => unitOfWork.CommitAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        CountSaved(database).Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_RowRemovedByAnotherWriter_IsAConcurrencyConflict()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using var context = database.CreateContext();
        var claim = await context.Claims.SingleAsync(saved => saved.Id == claimId);
        context.Entry(claim).Property(saved => saved.Notes).CurrentValue = "changed";
        await using (var other = database.CreateContext())
        {
            await other.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF; DELETE FROM Claims");
        }
        var unitOfWork = CreateUnitOfWork(database, context, new Mock<IEventDispatcher>());

        Func<Task> act = () => unitOfWork.CommitAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
    }

    [Fact]
    public async Task CommitAsync_ForeignKeyViolation_IsNotTranslated()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var unitOfWork = CreateUnitOfWork(database, context, new Mock<IEventDispatcher>());
        var orphan = TestReserves.Open(Ids.Next(), TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
        context.ReserveComponents.Add(orphan);

        Func<Task> act = () => unitOfWork.CommitAsync(CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    private static int CountSaved(TestDatabase database)
    {
        using var reader = database.CreateContext();

        return reader.Claims.Count();
    }

    private UnitOfWork CreateUnitOfWork(
        TestDatabase database,
        ClaimsDbContext context,
        Mock<IEventDispatcher> dispatcher)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(source => source.UtcNow).Returns(() => database.Now);
        var user = new Mock<ICurrentUser>();
        user.SetupGet(current => current.Id).Returns(TestDatabase.UserId);
        var correlation = new Mock<ICorrelationContext>();
        correlation.SetupGet(source => source.CorrelationId).Returns(_correlationId);

        return new UnitOfWork(context, dispatcher.Object, clock.Object, user.Object, correlation.Object);
    }
}
