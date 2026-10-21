namespace ClaimsModule.API.Tests.Support;

public sealed class ApiTheoryAttribute : TheoryAttribute
{
    public ApiTheoryAttribute()
    {
        if (!ApiFixture.IsAvailable && !ApiFixture.IsRequired)
        {
            Skip = "SQL Server is not reachable (set CLAIMS_TEST_SQLSERVER or start LocalDB).";
        }
    }
}
