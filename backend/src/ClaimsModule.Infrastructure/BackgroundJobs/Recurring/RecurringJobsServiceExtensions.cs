using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.BackgroundJobs.Recurring;

internal static class RecurringJobsServiceExtensions
{
    internal const string SlaJobId = "sla-breach-detection";
    internal const string PostingSweepJobId = "posting-sweep";

    public static IServiceCollection AddRecurringJobs(this IServiceCollection services)
    {
        services
            .AddOptions<SlaBreachOptions>()
            .BindConfiguration(SlaBreachOptions.SectionName)
            .Validate(
                options => IsMinuteInterval(options.IntervalMinutes),
                "The SLA breach interval must be a divisor of 60 minutes.")
            .ValidateOnStart();

        services
            .AddOptions<PostingSweepOptions>()
            .BindConfiguration(PostingSweepOptions.SectionName)
            .Validate(options => options.MinimumAge > TimeSpan.Zero, "The posting sweep minimum age must be positive.")
            .Validate(
                options => IsMinuteInterval(options.IntervalMinutes),
                "The posting sweep interval must be a divisor of 60 minutes.")
            .ValidateOnStart();

        services.AddScoped<SlaBreachJob>();
        services.AddScoped<PostingSweepJob>();

        return services;
    }

    public static IApplicationBuilder UseRecurringJobs(this IApplicationBuilder app)
    {
        var manager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();
        var sla = app.ApplicationServices.GetRequiredService<IOptions<SlaBreachOptions>>().Value;
        var sweep = app.ApplicationServices.GetRequiredService<IOptions<PostingSweepOptions>>().Value;

        manager.AddOrUpdate<SlaBreachJob>(
            SlaJobId,
            job => job.RunAsync(CancellationToken.None),
            Cron.MinuteInterval(sla.IntervalMinutes));
        manager.AddOrUpdate<PostingSweepJob>(
            PostingSweepJobId,
            job => job.RunAsync(CancellationToken.None),
            Cron.MinuteInterval(sweep.IntervalMinutes));

        return app;
    }

    private static bool IsMinuteInterval(int minutes)
    {
        bool isWithinAnHour = minutes is >= 1 and <= 59;

        return isWithinAnHour && 60 % minutes == 0;
    }
}
