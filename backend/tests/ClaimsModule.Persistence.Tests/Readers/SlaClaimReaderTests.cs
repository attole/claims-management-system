using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Persistence.Audit;
using ClaimsModule.Persistence.Readers;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Readers;

public sealed class SlaClaimReaderTests
{
    [Fact]
    [Trait("Rule", "S:JOB-SLA")]
    public async Task ListBreachedClaimIdsAsync_DraftClaimNotUpdatedSince_IsReturned()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        database.Now = database.Now.AddHours(49);
        using var context = database.CreateContext();
        var reader = new SlaClaimReader(context);

        var result = await reader.ListBreachedClaimIdsAsync(
            database.Now.AddHours(-48),
            database.Now.AddHours(-24),
            CancellationToken.None);

        result.Should().Equal(claimId);
    }

    [Fact]
    [Trait("Rule", "S:JOB-SLA")]
    public async Task ListBreachedClaimIdsAsync_ClaimUpdatedRecently_IsNotReturned()
    {
        using var database = new TestDatabase();
        var claim = TestClaims.Create(database);
        TestClaims.Save(database, claim);
        database.Now = database.Now.AddHours(47);
        using var context = database.CreateContext();
        var reader = new SlaClaimReader(context);

        var result = await reader.ListBreachedClaimIdsAsync(
            database.Now.AddHours(-48),
            database.Now.AddHours(-24),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:JOB-SLA")]
    public async Task ListBreachedClaimIdsAsync_ClaimThatIsNotDraftOrOpen_IsNotReturned()
    {
        using var database = new TestDatabase();
        TestClaims.Seed(database);
        using (var writer = database.CreateContext())
        {
            await writer.Database.ExecuteSqlRawAsync("UPDATE Claims SET Status = 'Closed'");
        }

        database.Now = database.Now.AddHours(49);
        using var context = database.CreateContext();
        var reader = new SlaClaimReader(context);

        var result = await reader.ListBreachedClaimIdsAsync(
            database.Now.AddHours(-48),
            database.Now.AddHours(-24),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:JOB-SLA")]
    public async Task ListBreachedClaimIdsAsync_RemovedClaim_IsNotReturned()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using (var writer = database.CreateContext())
        {
            writer.Claims.Remove(writer.Claims.Single(claim => claim.Id == claimId));
            writer.SaveChanges();
        }

        database.Now = database.Now.AddHours(49);
        using var context = database.CreateContext();
        var reader = new SlaClaimReader(context);

        var result = await reader.ListBreachedClaimIdsAsync(
            database.Now.AddHours(-48),
            database.Now.AddHours(-24),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:JOB-SLA-Dedupe")]
    public async Task ListBreachedClaimIdsAsync_ClaimFlaggedWithinTheWindow_IsNotReturned()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        database.Now = database.Now.AddHours(49);
        await WriteBreach(database, claimId);
        database.Now = database.Now.AddHours(1);
        using var context = database.CreateContext();
        var reader = new SlaClaimReader(context);

        var result = await reader.ListBreachedClaimIdsAsync(
            database.Now.AddHours(-48),
            database.Now.AddHours(-24),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:JOB-SLA-Dedupe")]
    public async Task ListBreachedClaimIdsAsync_ClaimFlaggedBeforeTheWindow_IsReturned()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        database.Now = database.Now.AddHours(49);
        await WriteBreach(database, claimId);
        database.Now = database.Now.AddHours(25);
        using var context = database.CreateContext();
        var reader = new SlaClaimReader(context);

        var result = await reader.ListBreachedClaimIdsAsync(
            database.Now.AddHours(-48),
            database.Now.AddHours(-24),
            CancellationToken.None);

        result.Should().Equal(claimId);
    }

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task ListBreachedClaimIdsAsync_JobWithNoOrganisation_SeesEveryOrganisation()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        database.Now = database.Now.AddHours(49);
        using var context = database.CreateContext(Guid.Empty, Guid.Empty);
        var reader = new SlaClaimReader(context);

        var result = await reader.ListBreachedClaimIdsAsync(
            database.Now.AddHours(-48),
            database.Now.AddHours(-24),
            CancellationToken.None);

        result.Should().Equal(claimId);
    }

    private static async Task WriteBreach(TestDatabase database, Guid claimId)
    {
        using var context = database.CreateContext();
        var service = new AuditLogService(context);
        var entry = new AuditEntry(claimId, AuditEventTypes.SlaBreachDetected, "Breach.");

        await service.WriteAsync(entry, new AuditContext(database.Now, Guid.Empty, Guid.NewGuid()), default);
        await context.SaveChangesAsync();
    }
}
