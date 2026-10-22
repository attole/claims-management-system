using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace ClaimsModule.API.Pipeline.Telemetry;

public static class TelemetryServiceExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(TelemetryOptions.SectionName);
        var options = section.Get<TelemetryOptions>() ?? new TelemetryOptions();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return services;
        }

        services
            .AddOpenTelemetry()
            .UseAzureMonitor(monitor => monitor.ConnectionString = options.ConnectionString);

        return services;
    }
}
