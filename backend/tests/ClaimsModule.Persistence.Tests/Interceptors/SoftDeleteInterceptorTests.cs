using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.CauseOfLoss;
using ClaimsModule.Persistence.Tests.Support.Interceptors;

namespace ClaimsModule.Persistence.Tests.Interceptors;

public sealed class SoftDeleteInterceptorTests
{
    private static readonly DateTimeOffset _later = new(2026, 3, 2, 9, 0, 0, TimeSpan.Zero);

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
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void SaveChanges_RemovedRowThatIsNotSoftDeletable_Throws()
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
