using Hangfire.Dashboard;

namespace ClaimsModule.Infrastructure.BackgroundJobs;

internal sealed class OpenDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
