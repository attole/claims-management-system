using Hangfire.Dashboard;

namespace ClaimsModule.Infrastructure.BackgroundJobs;

public sealed class OpenDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
