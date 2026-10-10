using ClaimsModule.Infrastructure.Database;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.BackgroundJobs;

internal static class BackgroundJobsServiceExtensions
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services
            .AddOptions<HangfireDashboardOptions>()
            .BindConfiguration(HangfireDashboardOptions.SectionName);

        services.AddHangfire((provider, hangfire) => hangfire
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ClaimsDb));
        services.AddHangfireServer();

        return services;
    }

    public static IApplicationBuilder UseBackgroundJobsDashboard(this IApplicationBuilder app)
    {
        var dashboard = app.ApplicationServices.GetRequiredService<IOptions<HangfireDashboardOptions>>().Value;

        if (!dashboard.Enabled)
        {
            return app;
        }

        return app.UseHangfireDashboard(
            options: new DashboardOptions { Authorization = [new OpenDashboardAuthorizationFilter()] });
    }
}
