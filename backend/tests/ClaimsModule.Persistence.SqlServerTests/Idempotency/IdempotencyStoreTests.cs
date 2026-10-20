using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Idempotency;
using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Idempotency;
using ClaimsModule.Persistence.Schema.Seed;
using ClaimsModule.Persistence.SqlServerTests.Support;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.SqlServerTests.Idempotency;

[Collection(SqlServerCollection.Name)]
public sealed class IdempotencyStoreTests(SqlServerFixture database)
{
    [SqlServerFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task PurgeExpiredAsync_KeyOlderThanTheWindow_IsDeletedWithoutAnOrganisation()
    {
        var organisationId = Guid.NewGuid();
        var request = new IdempotencyRequest(Guid.NewGuid().ToString("N"), "POST", "/api/claims", "hash");
        using (var setup = database.CreateContext(organisationId))
        {
            await CreateStore(setup, organisationId, SqlServerFixture.Now).BeginAsync(request, CancellationToken.None);
            await setup.SaveChangesAsync();
        }

        using var jobContext = database.CreateContext(Guid.Empty);
        var store = CreateStore(jobContext, Guid.Empty, SqlServerFixture.Now.AddHours(25));

        await store.PurgeExpiredAsync(CancellationToken.None);

        var remaining = await jobContext.IdempotencyRecords
            .IgnoreQueryFilters()
            .CountAsync(record => record.Key == request.Key);
        remaining.Should().Be(0);
    }

    [SqlServerFact]
    public async Task PurgeExpiredAsync_MoreExpiredKeysThanOneBatch_RemovesAllOfThem()
    {
        var organisationId = Guid.NewGuid();
        using (var setup = database.CreateContext(organisationId))
        {
            var records = Enumerable.Range(0, 1_100).Select(index => new IdempotencyRecord(
                Ids.Next(),
                organisationId,
                SeedIds.SystemUser,
                new IdempotencyRequest($"key-{index}", "POST", "/api/claims", "hash"),
                SqlServerFixture.Now));
            setup.IdempotencyRecords.AddRange(records);
            await setup.SaveChangesAsync();
        }

        using var jobContext = database.CreateContext(Guid.Empty);
        var store = CreateStore(jobContext, Guid.Empty, SqlServerFixture.Now.AddHours(25));

        await store.PurgeExpiredAsync(CancellationToken.None);

        var remaining = await jobContext.IdempotencyRecords
            .IgnoreQueryFilters()
            .CountAsync(record => record.OrganisationId == organisationId);
        remaining.Should().Be(0);
    }

    [SqlServerFact]
    public async Task PurgeExpiredAsync_KeyInsideTheWindow_IsKept()
    {
        var organisationId = Guid.NewGuid();
        var request = new IdempotencyRequest(Guid.NewGuid().ToString("N"), "POST", "/api/claims", "hash");
        using (var setup = database.CreateContext(organisationId))
        {
            await CreateStore(setup, organisationId, SqlServerFixture.Now).BeginAsync(request, CancellationToken.None);
            await setup.SaveChangesAsync();
        }

        using var jobContext = database.CreateContext(Guid.Empty);
        var store = CreateStore(jobContext, Guid.Empty, SqlServerFixture.Now.AddHours(23));

        await store.PurgeExpiredAsync(CancellationToken.None);

        var remaining = await jobContext.IdempotencyRecords
            .IgnoreQueryFilters()
            .CountAsync(record => record.Key == request.Key);
        remaining.Should().Be(1);
    }

    private static IdempotencyStore CreateStore(ClaimsDbContext context, Guid organisationId, DateTimeOffset now)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(source => source.UtcNow).Returns(now);

        return new IdempotencyStore(context, clock.Object, SqlServerFixture.CreateUser(organisationId));
    }
}
