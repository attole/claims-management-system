using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ClaimsModule.API.Pipeline.Health;

public static class HealthServiceExtensions
{
    public const string LivePath = "/health/live";
    public const string ReadyPath = "/health/ready";

    private const string ReadyTag = "ready";

    public static IServiceCollection AddHealth(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: [ReadyTag]);

        return services;
    }

    public static WebApplication UseHealth(this WebApplication app)
    {
        var live = new HealthCheckOptions { Predicate = _ => false };
        var ready = new HealthCheckOptions { Predicate = check => check.Tags.Contains(ReadyTag) };

        app.MapHealthChecks(LivePath, live).AllowAnonymous();
        app.MapHealthChecks(ReadyPath, ready).AllowAnonymous();

        return app;
    }
}
