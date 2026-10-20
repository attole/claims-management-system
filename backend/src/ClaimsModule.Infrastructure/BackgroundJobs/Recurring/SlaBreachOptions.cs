namespace ClaimsModule.Infrastructure.BackgroundJobs.Recurring;

public sealed class SlaBreachOptions
{
    public const string SectionName = "Hangfire:SlaBreach";

    public int IntervalMinutes { get; init; } = 15;
}
