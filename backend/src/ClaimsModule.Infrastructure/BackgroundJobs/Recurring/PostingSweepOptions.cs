namespace ClaimsModule.Infrastructure.BackgroundJobs.Recurring;

public sealed class PostingSweepOptions
{
    public const string SectionName = "Hangfire:PostingSweep";

    public TimeSpan MinimumAge { get; init; } = TimeSpan.FromMinutes(10);

    public int IntervalMinutes { get; init; } = 5;
}
