using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.CauseOfLoss;
using ClaimsModule.Persistence.Tests.Support.Conventions;
using ClaimsModule.Persistence.Tests.Support.ReserveComponents;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Interceptors;

public sealed class AuditSoftDeleteInterceptorTests
{
    private static readonly DateTimeOffset _later = new(2026, 3, 2, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Rule", "S:DATA-AuditColumns")]
    public void SaveChanges_AddedRow_StampsTheCreation()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var code = TestCauseCodes.Create(TestDatabase.OrganisationId, "HAIL");
        context.CauseOfLossCodes.Add(code);

        var result = context.SaveChanges();

        result.Should().Be(1);
        context.Entry(code).Property("CreatedAt").CurrentValue.Should().Be(database.Now);
        context.Entry(code).Property("UserCreated").CurrentValue.Should().Be(TestDatabase.UserId);
        context.Entry(code).Property("UpdatedAt").CurrentValue.Should().BeNull();
    }

    [Fact]
    [Trait("Rule", "S:DATA-AuditColumns")]
    public void SaveChanges_ModifiedRow_StampsTheModification()
    {
        using var database = new TestDatabase();
        var code = TestCauseCodes.Create(TestDatabase.OrganisationId, "HAIL");
        using (var writer = database.CreateContext())
        {
            writer.CauseOfLossCodes.Add(code);
            writer.SaveChanges();
        }

        database.Now = _later;
        using var context = database.CreateContext();
        var stored = context.CauseOfLossCodes.Single();
        context.Entry(stored).Property(row => row.Name).CurrentValue = "Wind damage";

        var result = context.SaveChanges();

        result.Should().Be(1);
        context.Entry(stored).Property("UpdatedAt").CurrentValue.Should().Be(_later);
        context.Entry(stored).Property("UserModified").CurrentValue.Should().Be(TestDatabase.UserId);
    }

    [Fact]
    [Trait("Rule", "S:DATA-AuditColumns")]
    public void SaveChanges_NoCurrentUser_StampsNoUser()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext(TestDatabase.OrganisationId, Guid.Empty);
        var code = TestCauseCodes.Create(TestDatabase.OrganisationId, "HAIL");
        context.CauseOfLossCodes.Add(code);

        var result = context.SaveChanges();

        result.Should().Be(1);
        context.Entry(code).Property("UserCreated").CurrentValue.Should().BeNull();
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void SaveChanges_RemovedRow_KeepsTheRowAsSoftDeleted()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var code = TestCauseCodes.Create(TestDatabase.OrganisationId, "HAIL");
        context.CauseOfLossCodes.Add(code);
        context.SaveChanges();
        database.Now = _later;
        context.CauseOfLossCodes.Remove(code);

        var result = context.SaveChanges();

        result.Should().Be(1);
        context.Entry(code).Property("IsDeleted").CurrentValue.Should().Be(true);
        context.Entry(code).Property("DeletedAt").CurrentValue.Should().Be(_later);
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void SaveChanges_RowRemovedByTheDomain_StampsTheDeletionTime()
    {
        using var database = new TestDatabase();
        using var context = database.CreateConventionContext();
        var row = new SoftDeletableRow(Ids.Next(), TestDatabase.OrganisationId);
        context.Add(row);
        context.SaveChanges();
        database.Now = _later;
        row.Remove();

        var result = context.SaveChanges();

        result.Should().Be(1);
        context.Entry(row).Property("DeletedAt").CurrentValue.Should().Be(_later);
    }

    [Fact]
    [Trait("Rule", "S:DATA-AuditColumns")]
    public void SaveChanges_NewChildRow_MarksItsRootModified()
    {
        using var database = new TestDatabase();
        var component = TestReserves.Open(Ids.Next(), TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
        using (var writer = database.CreateContext())
        {
            writer.ReserveComponents.Add(component);
            writer.SaveChanges();
        }

        database.Now = _later;
        using var context = database.CreateContext();
        var stored = context.ReserveComponents.Include(found => found.History).Single();
        TestReserves.Submit(stored, TestReserves.AutoApprovedAmount, _later);

        var result = context.SaveChanges();

        result.Should().Be(2);
        context.Entry(stored).Property("UpdatedAt").CurrentValue.Should().Be(_later);
        context.Entry(stored).Property("UserModified").CurrentValue.Should().Be(TestDatabase.UserId);
    }

    [Fact]
    [Trait("Rule", "S:DATA-AuditColumns")]
    public void SaveChanges_UntouchedRoot_IsNotWritten()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var component = TestReserves.Open(Ids.Next(), TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
        context.ReserveComponents.Add(component);
        context.SaveChanges();

        var result = context.SaveChanges();

        result.Should().Be(0);
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void SaveChanges_RemovedRowOfAnAppendOnlyTable_Throws()
    {
        using var database = new TestDatabase();
        using var context = database.CreateConventionContext();
        var row = new AppendOnlyRow(Ids.Next(), TestDatabase.OrganisationId);
        context.Add(row);
        context.SaveChanges();
        context.Remove(row);

        Func<int> act = () => context.SaveChanges();

        act.Should().Throw<InvalidOperationException>();
    }
}
