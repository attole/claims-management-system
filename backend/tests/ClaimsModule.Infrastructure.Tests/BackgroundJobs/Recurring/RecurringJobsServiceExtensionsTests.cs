using ClaimsModule.Infrastructure.BackgroundJobs.Recurring;
using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Tests.BackgroundJobs.Recurring;

public sealed class RecurringJobsServiceExtensionsTests
{
    [Theory]
    [Trait("Rule", "S:JOB-Registration")]
    [InlineData(RecurringJobsServiceExtensions.SlaJobId, typeof(SlaBreachJob), "*/20 * * * *")]
    [InlineData(RecurringJobsServiceExtensions.PostingSweepJobId, typeof(PostingSweepJob), "*/10 * * * *")]
    public void UseRecurringJobs_RegistersTheJobOnItsConfiguredCadence(string jobId, Type jobType, string cron)
    {
        var manager = new Mock<IRecurringJobManager>();
        var services = new ServiceCollection()
            .AddSingleton(manager.Object)
            .AddSingleton(Options.Create(new SlaBreachOptions { IntervalMinutes = 20 }))
            .AddSingleton(Options.Create(new PostingSweepOptions { IntervalMinutes = 10 }));
        var app = new ApplicationBuilder(services.BuildServiceProvider());

        app.UseRecurringJobs();

        manager.Verify(
            mock => mock.AddOrUpdate(
                jobId,
                It.Is<Job>(job => job.Type == jobType),
                cron,
                It.IsAny<RecurringJobOptions>()),
            Times.Once());
    }

    [Theory]
    [InlineData("Hangfire:PostingSweep:MinimumAge", "00:00:00")]
    [InlineData("Hangfire:PostingSweep:IntervalMinutes", "0")]
    [InlineData("Hangfire:PostingSweep:IntervalMinutes", "60")]
    [InlineData("Hangfire:PostingSweep:IntervalMinutes", "7")]
    public void AddRecurringJobs_WithAnInvalidSweepSetting_FailsOptionsValidation(string key, string value)
    {
        var provider = CreateProvider(new KeyValuePair<string, string?>(key, value));

        var act = () => provider.GetRequiredService<IOptions<PostingSweepOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    [Theory]
    [InlineData("0")]
    [InlineData("60")]
    [InlineData("7")]
    public void AddRecurringJobs_WithAnInvalidSlaInterval_FailsOptionsValidation(string value)
    {
        var provider = CreateProvider(new KeyValuePair<string, string?>("Hangfire:SlaBreach:IntervalMinutes", value));

        var act = () => provider.GetRequiredService<IOptions<SlaBreachOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    [Trait("Rule", "S:JOB-SLA")]
    public void AddRecurringJobs_WithoutConfiguration_RunsTheSlaJobEveryFifteenMinutes()
    {
        var provider = CreateProvider();

        var result = provider.GetRequiredService<IOptions<SlaBreachOptions>>().Value;

        result.IntervalMinutes.Should().Be(15);
    }

    [Fact]
    public void AddRecurringJobs_WithoutConfiguration_RunsTheSweepEveryFiveMinutes()
    {
        var provider = CreateProvider();

        var result = provider.GetRequiredService<IOptions<PostingSweepOptions>>().Value;

        result.IntervalMinutes.Should().Be(5);
    }

    [Fact]
    public void AddRecurringJobs_WithoutConfiguration_SweepsRowsOlderThanTenMinutes()
    {
        var provider = CreateProvider();

        var result = provider.GetRequiredService<IOptions<PostingSweepOptions>>().Value;

        result.MinimumAge.Should().Be(TimeSpan.FromMinutes(10));
    }

    private static ServiceProvider CreateProvider(params KeyValuePair<string, string?>[] settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddRecurringJobs()
            .BuildServiceProvider();
    }
}
