using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Paging;
using ClaimsModule.Persistence.Audit;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;

namespace ClaimsModule.Persistence.Tests.Audit;

public sealed class AuditLogReaderTests
{
    [Fact]
    public async Task ListAsync_Claim_ReturnsOnlyItsEntriesNewestFirstWithTheStamp()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        var otherId = TestClaims.Seed(database, 2);
        var correlationId = Guid.NewGuid();
        await Write(database, claimId, "first", correlationId);
        database.Now = database.Now.AddHours(1);
        await Write(database, claimId, "second", correlationId);
        await Write(database, otherId, "other", correlationId);
        using var context = database.CreateContext();
        var reader = new AuditLogReader(context);

        var result = await reader.ListAsync(claimId, new PageWindow(1, 10), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Select(item => item.Description).Should().Equal("second", "first");
        result.Items[0].CreatedAt.Should().Be(database.Now);
        result.Items[0].CreatedByUserId.Should().Be(TestDatabase.UserId);
        result.Items[0].CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task ListAsync_EntryWithoutOptionalValues_ReadsThemAsEmpty()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        await Write(database, claimId, "plain", Guid.Empty);
        using var context = database.CreateContext();
        var reader = new AuditLogReader(context);

        var result = await reader.ListAsync(claimId, new PageWindow(1, 10), CancellationToken.None);

        var item = result.Items.Single();
        item.OldValue.Should().BeEmpty();
        item.RelatedEntityId.Should().Be(Guid.Empty);
        item.RelatedEntityType.Should().BeEmpty();
        item.CorrelationId.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task ListAsync_SecondPage_ReturnsTheRemainingEntries()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);

        foreach (var name in new[] { "a", "b", "c" })
        {
            database.Now = database.Now.AddMinutes(1);
            await Write(database, claimId, name, Guid.Empty);
        }

        using var context = database.CreateContext();
        var reader = new AuditLogReader(context);

        var result = await reader.ListAsync(claimId, new PageWindow(2, 2), CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Select(item => item.Description).Should().Equal("a");
    }

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task ListAsync_EntryOfAnotherOrganisation_IsHidden()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        await Write(database, claimId, "mine", Guid.Empty);
        using var context = database.CreateContext(Guid.NewGuid(), TestDatabase.UserId);
        var reader = new AuditLogReader(context);

        var result = await reader.ListAsync(claimId, new PageWindow(1, 10), CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    private static async Task Write(TestDatabase database, Guid claimId, string description, Guid correlationId)
    {
        using var context = database.CreateContext();
        var service = new AuditLogService(context);
        var entry = new AuditEntry(claimId, AuditEventTypes.ClaimCreated, description);

        await service.WriteAsync(entry, new AuditContext(database.Now, TestDatabase.UserId, correlationId), default);
        await context.SaveChangesAsync();
    }
}
