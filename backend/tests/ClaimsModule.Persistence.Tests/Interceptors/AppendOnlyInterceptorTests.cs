using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Audit;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Interceptors;

public sealed class AppendOnlyInterceptorTests
{
    [Fact]
    [Trait("Rule", "S:AUD-NoUpdateDelete")]
    public void SaveChanges_ChangedAuditLogRow_Throws()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using var context = database.CreateContext();
        var row = new AuditLogEntry(Ids.Next(), TestDatabase.OrganisationId, claimId, "CLAIM_CREATED", "created");
        context.AuditLog.Add(row);
        context.SaveChanges();
        context.Entry(row).Property(stored => stored.Description).CurrentValue = "edited";

        Func<int> act = () => context.SaveChanges();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [Trait("Rule", "S:AUD-NoUpdateDelete")]
    public void SaveChanges_RemovedAuditLogRow_Throws()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using var context = database.CreateContext();
        var row = new AuditLogEntry(Ids.Next(), TestDatabase.OrganisationId, claimId, "CLAIM_CREATED", "created");
        context.AuditLog.Add(row);
        context.SaveChanges();
        context.AuditLog.Remove(row);

        Func<int> act = () => context.SaveChanges();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SaveChanges_ChangedStateChangeRow_Throws()
    {
        using var database = new TestDatabase();
        TestClaims.Save(database, TestClaims.Opened(database));
        using var context = database.CreateContext();
        var stored = context.Claims.Include(claim => claim.StateChanges).Single();
        context.Entry(stored.StateChanges[0]).Property(change => change.Note).CurrentValue = "edited";

        Func<int> act = () => context.SaveChanges();

        act.Should().Throw<InvalidOperationException>();
    }
}
