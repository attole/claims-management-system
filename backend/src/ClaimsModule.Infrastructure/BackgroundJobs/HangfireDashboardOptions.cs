namespace ClaimsModule.Infrastructure.BackgroundJobs;

public sealed class HangfireDashboardOptions
{
    public const string SectionName = "Hangfire:Dashboard";

    public bool Enabled { get; init; } = false;
}
