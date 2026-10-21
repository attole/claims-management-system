using ClaimsModule.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClaimsModule.API.Pipeline.Health;

internal sealed class DatabaseHealthCheck(ClaimsDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthContext,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await context.Database.CanConnectAsync(cancellationToken);

        return canConnect ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}
