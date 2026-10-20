using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.BackgroundJobs.Recurring;

internal static class RecurringJobsServiceExtensions
{
    internal const string SlaJobId = "sla-breach-detection";
    internal const string PostingSweepJobId = "posting-sweep";
    internal const string IdempotencyCleanupJobId = "idempotency-cleanup";
    internal const string BlobSweepJobId = "blob-sweep";

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

        services
            .AddOptions<IdempotencyCleanupOptions>()
            .BindConfiguration(IdempotencyCleanupOptions.SectionName)
            .Validate(
                options => IsHourInterval(options.IntervalHours),
                "The idempotency cleanup interval must be 1, 2, 3, 4, 6, 8 or 12 hours.")
            .ValidateOnStart();

        services
            .AddOptions<BlobSweepOptions>()
            .BindConfiguration(BlobSweepOptions.SectionName)
            .Validate(options => options.MinimumAge > TimeSpan.Zero, "The blob sweep minimum age must be positive.")
            .Validate(
                options => IsHourInterval(options.IntervalHours),
                "The blob sweep interval must be 1, 2, 3, 4, 6, 8 or 12 hours.")
            .ValidateOnStart();

        services.AddScoped<SlaBreachJob>();
        services.AddScoped<PostingSweepJob>();
        services.AddScoped<IdempotencyCleanupJob>();
        services.AddScoped<BlobSweepJob>();

        return services;
    }

    public static IApplicationBuilder UseRecurringJobs(this IApplicationBuilder app)
    {
        var provider = app.ApplicationServices;
        var manager = provider.GetRequiredService<IRecurringJobManager>();
        var sla = provider.GetRequiredService<IOptions<SlaBreachOptions>>().Value;
        var sweep = provider.GetRequiredService<IOptions<PostingSweepOptions>>().Value;
        var cleanup = provider.GetRequiredService<IOptions<IdempotencyCleanupOptions>>().Value;
        var blobs = provider.GetRequiredService<IOptions<BlobSweepOptions>>().Value;

        manager.AddOrUpdate<SlaBreachJob>(
            SlaJobId,
            job => job.RunAsync(CancellationToken.None),
            Cron.MinuteInterval(sla.IntervalMinutes));
        manager.AddOrUpdate<PostingSweepJob>(
            PostingSweepJobId,
            job => job.RunAsync(CancellationToken.None),
            Cron.MinuteInterval(sweep.IntervalMinutes));
        manager.AddOrUpdate<IdempotencyCleanupJob>(
            IdempotencyCleanupJobId,
            job => job.RunAsync(CancellationToken.None),
            Cron.HourInterval(cleanup.IntervalHours));

        if (blobs.Enabled)
        {
            manager.AddOrUpdate<BlobSweepJob>(
                BlobSweepJobId,
                job => job.RunAsync(CancellationToken.None),
                Cron.HourInterval(blobs.IntervalHours));
        }
        else
        {
            manager.RemoveIfExists(BlobSweepJobId);
        }

        return app;
    }

    private static bool IsMinuteInterval(int minutes)
    {
        bool isWithinAnHour = minutes is >= 1 and <= 59;

        return isWithinAnHour && 60 % minutes == 0;
    }

    private static bool IsHourInterval(int hours)
    {
        bool isWithinHalfADay = hours is >= 1 and <= 12;

        return isWithinHalfADay && 24 % hours == 0;
    }
}
