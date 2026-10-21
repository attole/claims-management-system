namespace ClaimsModule.API.Tests.Support;

public sealed class ApiFactAttribute : FactAttribute
{
    public ApiFactAttribute()
    {
        if (!ApiFixture.IsAvailable && !ApiFixture.IsRequired)
        {
            Skip = "SQL Server is not reachable (set CLAIMS_TEST_SQLSERVER or start LocalDB).";
        }
    }
}
