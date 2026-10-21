using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Idempotency;
using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Idempotency;
using ClaimsModule.Persistence.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Idempotency;

public sealed class IdempotencyStoreTests
{
    private static readonly IdempotencyRequest _request = new("key-1", "POST", "/api/claims", "hash-a");

    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task BeginAsync_NewKey_StartsAndStagesTheRowWithoutSavingIt()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var store = CreateStore(database, context);

        var outcome = await store.BeginAsync(_request, CancellationToken.None);

        outcome.Status.Should().Be(IdempotencyStatus.Started);
        CountSaved(database).Should().Be(0);
        context.IdempotencyRecords.Local.Should().ContainSingle();
    }

    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task BeginAsync_KeySavedWithTheCommandButNoResponse_IsInProgress()
    {
        using var database = new TestDatabase();
        await BeginAndCommit(database, _request);

        var result = await Begin(database, _request);

        result.Status.Should().Be(IdempotencyStatus.InProgress);
    }

    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task BeginAsync_CompletedKey_ReturnsTheStoredResponse()
    {
        using var database = new TestDatabase();
        await BeginAndCommit(database, _request);
        await Complete(database, _request, new IdempotencyResponse(201, "{\"id\":1}"));

        var result = await Begin(database, _request);

        result.Status.Should().Be(IdempotencyStatus.Completed);
        result.Stored.Should().Be(new IdempotencyResponse(201, "{\"id\":1}"));
    }

    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task BeginAsync_SameKeyWithAnotherBody_IsABodyMismatch()
    {
        using var database = new TestDatabase();
        await BeginAndCommit(database, _request);

        var result = await Begin(database, _request with { BodyHash = "hash-b" });

        result.Status.Should().Be(IdempotencyStatus.BodyMismatch);
    }

    [Theory]
    [InlineData("PUT", "/api/claims")]
    [InlineData("POST", "/api/other")]
    public async Task BeginAsync_SameKeyOnAnotherMethodOrRoute_Starts(string method, string route)
    {
        using var database = new TestDatabase();
        await BeginAndCommit(database, _request);

        var result = await Begin(database, _request with { Method = method, Route = route });

        result.Status.Should().Be(IdempotencyStatus.Started);
    }

    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task BeginAsync_SameKeyFromAnotherUser_Starts()
    {
        using var database = new TestDatabase();
        await BeginAndCommit(database, _request);

        var result = await Begin(database, _request, TestDatabase.OrganisationId, Guid.NewGuid());

        result.Status.Should().Be(IdempotencyStatus.Started);
    }

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task BeginAsync_SameKeyFromAnotherOrganisation_Starts()
    {
        using var database = new TestDatabase();
        await BeginAndCommit(database, _request);

        var result = await Begin(database, _request, Guid.NewGuid(), TestDatabase.UserId);

        result.Status.Should().Be(IdempotencyStatus.Started);
    }

    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task BeginAsync_ExpiredKey_CountsAsNew()
    {
        using var database = new TestDatabase();
        await BeginAndCommit(database, _request);
        await Complete(database, _request, new IdempotencyResponse(201, "{}"));
        database.Now = database.Now.AddHours(25);

        var result = await Begin(database, _request with { BodyHash = "hash-b" });

        result.Status.Should().Be(IdempotencyStatus.Started);
    }

    [Fact]
    public async Task BeginAsync_TwoRequestsWithTheSameKey_OnlyTheFirstCommitSucceeds()
    {
        using var database = new TestDatabase();
        using var first = database.CreateContext();
        using var second = database.CreateContext();
        await CreateStore(database, first).BeginAsync(_request, CancellationToken.None);
        await CreateStore(database, second).BeginAsync(_request, CancellationToken.None);
        await first.SaveChangesAsync();

        Func<Task> act = () => second.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task CompleteAsync_KeyStagedButNotCommitted_SavesItWithTheResponse()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var store = CreateStore(database, context);
        await store.BeginAsync(_request, CancellationToken.None);

        await store.CompleteAsync(_request, new IdempotencyResponse(422, "{}"), CancellationToken.None);

        var result = await Begin(database, _request);
        result.Stored.Should().Be(new IdempotencyResponse(422, "{}"));
    }

    [Fact]
    public async Task CompleteAsync_KeyNeverBegun_Throws()
    {
        using var database = new TestDatabase();

        Func<Task> act = () => Complete(database, _request, new IdempotencyResponse(200, "{}"));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task PurgeExpiredAsync_RemovesKeysOlderThanTheWindowOfEveryOrganisation()
    {
        using var database = new TestDatabase();
        var otherOrganisation = Guid.NewGuid();
        await BeginAndCommit(database, _request);
        using (var other = database.CreateContext(otherOrganisation, TestDatabase.UserId))
        {
            var otherStore = CreateStore(database, other, otherOrganisation, TestDatabase.UserId);
            await otherStore.BeginAsync(_request, CancellationToken.None);
            await other.SaveChangesAsync();
        }

        database.Now = database.Now.AddHours(25);
        using var jobContext = database.CreateContext(Guid.Empty, Guid.Empty);
        var store = CreateStore(database, jobContext, Guid.Empty, Guid.Empty);

        int result = await store.PurgeExpiredAsync(CancellationToken.None);

        result.Should().Be(2);
    }

    [Fact]
    public async Task PurgeExpiredAsync_MoreExpiredKeysThanOneBatch_RemovesAllOfThem()
    {
        using var database = new TestDatabase();
        using (var setup = database.CreateContext())
        {
            var records = Enumerable.Range(0, 1_100).Select(index => new IdempotencyRecord(
                Ids.Next(),
                TestDatabase.OrganisationId,
                TestDatabase.UserId,
                _request with { Key = $"key-{index}" },
                database.Now));
            setup.IdempotencyRecords.AddRange(records);
            await setup.SaveChangesAsync();
        }

        database.Now = database.Now.AddHours(25);
        using var jobContext = database.CreateContext(Guid.Empty, Guid.Empty);
        var store = CreateStore(database, jobContext, Guid.Empty, Guid.Empty);

        int result = await store.PurgeExpiredAsync(CancellationToken.None);

        result.Should().Be(1_100);
    }

    [Fact]
    public async Task PurgeExpiredAsync_KeyInsideTheWindow_IsKept()
    {
        using var database = new TestDatabase();
        await BeginAndCommit(database, _request);
        database.Now = database.Now.AddHours(23);
        using var jobContext = database.CreateContext(Guid.Empty, Guid.Empty);
        var store = CreateStore(database, jobContext, Guid.Empty, Guid.Empty);

        await store.PurgeExpiredAsync(CancellationToken.None);

        CountSaved(database).Should().Be(1);
    }

    private static int CountSaved(TestDatabase database)
    {
        using var reader = database.CreateContext();

        return reader.IdempotencyRecords.Count();
    }

    private static async Task BeginAndCommit(TestDatabase database, IdempotencyRequest request)
    {
        using var context = database.CreateContext();
        await CreateStore(database, context).BeginAsync(request, CancellationToken.None);

        await context.SaveChangesAsync();
    }

    private static async Task<IdempotencyDecision> Begin(
        TestDatabase database,
        IdempotencyRequest request,
        Guid? organisationId = null,
        Guid? userId = null)
    {
        var organisation = organisationId ?? TestDatabase.OrganisationId;
        var user = userId ?? TestDatabase.UserId;
        using var context = database.CreateContext(organisation, user);
        var store = CreateStore(database, context, organisation, user);

        return await store.BeginAsync(request, CancellationToken.None);
    }

    private static async Task Complete(
        TestDatabase database,
        IdempotencyRequest request,
        IdempotencyResponse response)
    {
        using var context = database.CreateContext();

        await CreateStore(database, context).CompleteAsync(request, response, CancellationToken.None);
    }

    private static IdempotencyStore CreateStore(TestDatabase database, ClaimsDbContext context)
    {
        return CreateStore(database, context, TestDatabase.OrganisationId, TestDatabase.UserId);
    }

    private static IdempotencyStore CreateStore(
        TestDatabase database,
        ClaimsDbContext context,
        Guid organisationId,
        Guid userId)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(source => source.UtcNow).Returns(() => database.Now);
        var user = new Mock<ICurrentUser>();
        user.SetupGet(current => current.Id).Returns(userId);
        user.SetupGet(current => current.OrganisationId).Returns(organisationId);

        return new IdempotencyStore(context, clock.Object, user.Object);
    }
}
