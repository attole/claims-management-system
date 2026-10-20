namespace ClaimsModule.Persistence.SqlServerTests.Support;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (!SqlServerFixture.IsAvailable)
        {
            Skip = "SQL Server is not reachable (set CLAIMS_TEST_SQLSERVER or start LocalDB).";
        }
    }
}
