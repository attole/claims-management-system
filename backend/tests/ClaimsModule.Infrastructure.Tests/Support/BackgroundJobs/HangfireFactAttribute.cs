namespace ClaimsModule.Infrastructure.Tests.Support.BackgroundJobs;

public sealed class HangfireFactAttribute : FactAttribute
{
    public HangfireFactAttribute()
    {
        if (!HangfireTestHost.IsAvailable && !HangfireTestHost.IsRequired)
        {
            Skip = "SQL Server is not reachable (set CLAIMS_TEST_SQLSERVER or start LocalDB).";
        }
    }
}
