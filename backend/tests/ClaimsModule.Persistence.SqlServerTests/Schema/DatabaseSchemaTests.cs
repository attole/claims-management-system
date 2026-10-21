using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Audit;
using ClaimsModule.Persistence.Rows;
using ClaimsModule.Persistence.Schema;
using ClaimsModule.Persistence.Schema.Seed;
using ClaimsModule.Persistence.SqlServerTests.Support;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.SqlServerTests.Schema;

[Collection(SqlServerCollection.Name)]
public sealed class DatabaseSchemaTests(SqlServerFixture database)
{
    private const int PermissionDenied = 229;

    private const int ForeignKeyConflict = 547;

    [SqlServerFact]
    public void Migrations_AllApplied_LeaveNoPendingModelChanges()
    {
        using var context = database.CreateContext(SeedIds.Organisation);

        var result = context.Database.HasPendingModelChanges();

        result.Should().BeFalse();
    }

    [SqlServerFact]
    [Trait("Rule", "S:POL-Seed")]
    [Trait("Rule", "S:DATA-Seed")]
    public void Seed_AfterMigrating_HasFivePoliciesWithNineCoverageTypes()
    {
        using var context = database.CreateContext(SeedIds.Organisation);

        var result = (
            Policies: context.Policies.Count(),
            CoverageTypes: context.PolicyCoverageTypes.Count());

        result.Should().Be((5, 9));
    }

    [SqlServerFact]
    [Trait("Rule", "S:COL-Seed")]
    [Trait("Rule", "S:DATA-Seed")]
    public void Seed_AfterMigrating_HasTenActiveCauseOfLossCodes()
    {
        using var context = database.CreateContext(SeedIds.Organisation);

        var result = context.CauseOfLossCodes.Count(code => code.IsActive);

        result.Should().Be(10);
    }

    [SqlServerFact]
    public void ClaimListView_ClaimLinkedToASeededPolicy_ShowsThePolicyNumberAndClient()
    {
        using var context = database.CreateContext(SeedIds.Organisation);
        var claim = SqlServerClaims.CreateLinked(SeedIds.Organisation, SeedIds.For("POL-2024-001001"));
        context.Claims.Add(claim);
        context.SaveChanges();

        var result = context.Set<ClaimListRow>().Single(row => row.ClaimId == claim.Id);

        result.PolicyNumber.Should().Be("POL-2024-001001");
        result.ClientName.Should().Be("Meridian Transport LLC");
    }

    [SqlServerFact]
    public void ClaimListView_AfterTheClaimIsRemoved_NoLongerShowsIt()
    {
        using var context = database.CreateContext(SeedIds.Organisation);
        var claim = SqlServerClaims.Create(SeedIds.Organisation);
        context.Claims.Add(claim);
        context.SaveChanges();
        context.Claims.Remove(claim);
        context.SaveChanges();

        var result = context.Set<ClaimListRow>().Count(row => row.ClaimId == claim.Id);

        result.Should().Be(0);
    }

    [SqlServerFact]
    public void ClaimListView_ClaimsSaved_MatchTheSelectItWasBuiltFrom()
    {
        using var context = database.CreateContext(SeedIds.Organisation);
        context.Claims.Add(SqlServerClaims.CreateLinked(SeedIds.Organisation, SeedIds.For("POL-2025-002001")));
        context.SaveChanges();
        var fromSelect = context.Database
            .SqlQueryRaw<int>($"SELECT COUNT(*) AS Value FROM ({ClaimListView.SelectSql}) AS list")
            .Single();

        var result = context.Set<ClaimListRow>().IgnoreQueryFilters().Count();

        result.Should().Be(fromSelect);
    }

    [SqlServerTheory]
    [Trait("Rule", "S:AUD-NoUpdateDelete")]
    [InlineData("UPDATE dbo.ClaimAuditLog SET Description = 'changed'")]
    [InlineData("DELETE FROM dbo.ClaimAuditLog")]
    public void AuditLog_UserInTheAppRole_IsDeniedUpdateAndDelete(string statement)
    {
        Action act = () => RunAsAppRoleMember(statement);

        act.Should().Throw<SqlException>().Which.Number.Should().Be(PermissionDenied);
    }

    [SqlServerFact]
    [Trait("Rule", "S:AUD-NoUpdateDelete")]
    public void Claims_HardDeleteOfAClaimWithAuditRows_IsRefusedByTheForeignKey()
    {
        using var context = database.CreateContext(SeedIds.Organisation);
        var claim = SqlServerClaims.Create(SeedIds.Organisation);
        context.Claims.Add(claim);
        context.AuditLog.Add(new AuditLogEntry(Ids.Next(), SeedIds.Organisation, claim.Id, "CLAIM_CREATED", "created"));
        context.SaveChanges();

        Action act = () => context.Database.ExecuteSqlInterpolated($"DELETE FROM Claims WHERE ClaimId = {claim.Id}");

        act.Should().Throw<SqlException>().Which.Number.Should().Be(ForeignKeyConflict);
    }

    [SqlServerFact]
    [Trait("Rule", "S:AUD-NoUpdateDelete")]
    public void AuditLog_UserInTheAppRole_CanStillRead()
    {
        Action act = () => RunAsAppRoleMember("SELECT COUNT(*) FROM dbo.ClaimAuditLog");

        act.Should().NotThrow();
    }

    private static void Execute(SqlConnection connection, string sql)
    {
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    private void RunAsAppRoleMember(string statement)
    {
        using var connection = new SqlConnection(database.ConnectionString);
        connection.Open();
        Execute(connection, "IF DATABASE_PRINCIPAL_ID('audit_tester') IS NULL CREATE USER audit_tester WITHOUT LOGIN");
        Execute(
            connection,
            "IF IS_ROLEMEMBER('claims_app', 'audit_tester') = 0 ALTER ROLE claims_app ADD MEMBER audit_tester");
        Execute(connection, "GRANT SELECT, UPDATE, DELETE ON dbo.ClaimAuditLog TO audit_tester");
        Execute(connection, "EXECUTE AS USER = 'audit_tester'");

        try
        {
            Execute(connection, statement);
        }
        finally
        {
            Execute(connection, "REVERT");
        }
    }
}
