namespace ClaimsModule.Infrastructure.Tests.Support.BackgroundJobs;

[CollectionDefinition(Name)]
public sealed class HangfireCollection
{
    public const string Name = "Hangfire";
}
