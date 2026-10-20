namespace ClaimsModule.Infrastructure.BackgroundJobs.Recurring;

public sealed class BlobSweepOptions
{
    public const string SectionName = "Hangfire:BlobSweep";

    public bool Enabled { get; init; } = false;

    public TimeSpan MinimumAge { get; init; } = TimeSpan.FromDays(1);

    public int IntervalHours { get; init; } = 12;
}
