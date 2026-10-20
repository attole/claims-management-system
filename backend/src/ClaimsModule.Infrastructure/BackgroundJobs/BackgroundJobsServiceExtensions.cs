using ClaimsModule.Application.Abstractions;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.BackgroundJobs;

internal static class BackgroundJobsServiceExtensions
{
    private const string ConnectionName = "ClaimsDb";

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services
            .AddOptions<HangfireDashboardOptions>()
            .BindConfiguration(HangfireDashboardOptions.SectionName);

        services.AddHangfire((provider, hangfire) => hangfire
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(ReadConnectionString(provider)));
        services.AddHangfireServer();
        services.AddScoped<ReservePostingJob>();
        services.AddScoped<IPostingScheduler, HangfirePostingScheduler>();

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

    private static string ReadConnectionString(IServiceProvider provider)
    {
        var configuration = provider.GetRequiredService<IConfiguration>();

        return configuration.GetConnectionString(ConnectionName) ?? string.Empty;
    }
}
