namespace ClaimsModule.Persistence.SqlServerTests.Support;

public sealed class SqlServerTheoryAttribute : TheoryAttribute
{
    public SqlServerTheoryAttribute()
    {
        if (!SqlServerFixture.IsAvailable && !SqlServerFixture.IsRequired)
        {
            Skip = "SQL Server is not reachable (set CLAIMS_TEST_SQLSERVER or start LocalDB).";
        }
    }
}
