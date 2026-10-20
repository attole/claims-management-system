using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.CauseOfLoss;

namespace ClaimsModule.Persistence.Tests.Interceptors;

public sealed class RowStampInterceptorTests
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
}
