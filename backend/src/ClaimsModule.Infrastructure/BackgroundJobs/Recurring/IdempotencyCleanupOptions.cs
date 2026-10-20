namespace ClaimsModule.Infrastructure.BackgroundJobs.Recurring;

public sealed class IdempotencyCleanupOptions
{
    public const string SectionName = "Hangfire:IdempotencyCleanup";

    public int IntervalHours { get; init; } = 1;
}
