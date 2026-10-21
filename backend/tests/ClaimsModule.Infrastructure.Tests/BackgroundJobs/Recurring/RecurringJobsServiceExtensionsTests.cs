using ClaimsModule.Infrastructure.BackgroundJobs.Recurring;
using ClaimsModule.Infrastructure.Tests.Support.BackgroundJobs;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Tests.BackgroundJobs.Recurring;

[Collection(HangfireCollection.Name)]
public sealed class RecurringJobsServiceExtensionsTests : IAsyncLifetime
{
    private readonly List<HangfireTestHost> _databases = [];

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var database in _databases)
        {
            await database.DisposeAsync();
        }
    }

    [Theory]
    [Trait("Rule", "S:JOB-Registration")]
    [InlineData(RecurringJobsServiceExtensions.SlaJobId, typeof(SlaBreachJob), "*/20 * * * *")]
    [InlineData(RecurringJobsServiceExtensions.PostingSweepJobId, typeof(PostingSweepJob), "*/10 * * * *")]
    [InlineData(RecurringJobsServiceExtensions.IdempotencyCleanupJobId, typeof(IdempotencyCleanupJob), "0 */2 * * *")]
    [InlineData(RecurringJobsServiceExtensions.BlobSweepJobId, typeof(BlobSweepJob), "0 */4 * * *")]
    public void UseRecurringJobs_RegistersTheJobOnItsConfiguredCadence(string jobId, Type jobType, string cron)
    {
        var manager = new Mock<IRecurringJobManager>();
        var app = CreateApp(manager, new BlobSweepOptions { Enabled = true, IntervalHours = 4 });

        app.UseRecurringJobs();

        manager.Verify(
            mock => mock.AddOrUpdate(
                jobId,
                It.Is<Job>(job => job.Type == jobType),
                cron,
                It.IsAny<RecurringJobOptions>()),
            Times.Once());
    }

    [Fact]
    public void UseRecurringJobs_WithTheBlobSweepDisabled_RemovesItsDefinition()
    {
        var manager = new Mock<IRecurringJobManager>();
        var app = CreateApp(manager, new BlobSweepOptions { Enabled = false });

        app.UseRecurringJobs();

        manager.Verify(mock => mock.RemoveIfExists(RecurringJobsServiceExtensions.BlobSweepJobId), Times.Once());
    }

    [Fact]
    public void UseRecurringJobs_WithTheBlobSweepDisabled_DoesNotScheduleIt()
    {
        var manager = new Mock<IRecurringJobManager>();
        var app = CreateApp(manager, new BlobSweepOptions { Enabled = false });

        app.UseRecurringJobs();

        manager.Verify(
            mock => mock.AddOrUpdate(
                RecurringJobsServiceExtensions.BlobSweepJobId,
                It.IsAny<Job>(),
                It.IsAny<string>(),
                It.IsAny<RecurringJobOptions>()),
            Times.Never());
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

    [Theory]
    [InlineData("0")]
    [InlineData("5")]
    [InlineData("24")]
    public void AddRecurringJobs_WithAnInvalidCleanupInterval_FailsOptionsValidation(string value)
    {
        var key = "Hangfire:IdempotencyCleanup:IntervalHours";
        var provider = CreateProvider(new KeyValuePair<string, string?>(key, value));

        var act = () => provider.GetRequiredService<IOptions<IdempotencyCleanupOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    [Theory]
    [InlineData("Hangfire:BlobSweep:MinimumAge", "00:00:00")]
    [InlineData("Hangfire:BlobSweep:IntervalHours", "0")]
    [InlineData("Hangfire:BlobSweep:IntervalHours", "5")]
    public void AddRecurringJobs_WithAnInvalidBlobSweepSetting_FailsOptionsValidation(string key, string value)
    {
        var provider = CreateProvider(new KeyValuePair<string, string?>(key, value));

        var act = () => provider.GetRequiredService<IOptions<BlobSweepOptions>>().Value;

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

    [Fact]
    public void AddRecurringJobs_WithoutConfiguration_CleansIdempotencyKeysHourly()
    {
        var provider = CreateProvider();

        var result = provider.GetRequiredService<IOptions<IdempotencyCleanupOptions>>().Value;

        result.IntervalHours.Should().Be(1);
    }

    [Fact]
    public void AddRecurringJobs_WithoutConfiguration_LeavesTheBlobSweepOff()
    {
        var provider = CreateProvider();

        var result = provider.GetRequiredService<IOptions<BlobSweepOptions>>().Value;

        result.Enabled.Should().BeFalse();
    }

    [Fact]
    public void AddRecurringJobs_WithoutConfiguration_SweepsBlobsOlderThanADay()
    {
        var provider = CreateProvider();

        var result = provider.GetRequiredService<IOptions<BlobSweepOptions>>().Value;

        result.MinimumAge.Should().Be(TimeSpan.FromDays(1));
    }

    [Fact]
    public void AddRecurringJobs_WithoutConfiguration_SweepsBlobsEveryTwelveHours()
    {
        var provider = CreateProvider();

        var result = provider.GetRequiredService<IOptions<BlobSweepOptions>>().Value;

        result.IntervalHours.Should().Be(12);
    }

    private static ApplicationBuilder CreateApp(Mock<IRecurringJobManager> manager, BlobSweepOptions blobs)
    {
        var services = new ServiceCollection()
            .AddSingleton(manager.Object)
            .AddSingleton(Options.Create(new SlaBreachOptions { IntervalMinutes = 20 }))
            .AddSingleton(Options.Create(new PostingSweepOptions { IntervalMinutes = 10 }))
            .AddSingleton(Options.Create(new IdempotencyCleanupOptions { IntervalHours = 2 }))
            .AddSingleton(Options.Create(blobs));

        return new ApplicationBuilder(services.BuildServiceProvider());
    }

    private static ServiceProvider CreateProvider(params KeyValuePair<string, string?>[] settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddRecurringJobs()
            .BuildServiceProvider();
    }

    [HangfireFact]
    [Trait("Rule", "S:JOB-Registration")]
    public async Task UseRecurringJobs_OnARealDatabase_StoresTheThreeJobsOnTheirDefaultCadence()
    {
        var database = await CreateDatabaseAsync();
        var host = database.Build(new Dictionary<string, string?>(), Mock.Of<MediatR.IMediator>());
        var app = new ApplicationBuilder(host.Services);

        app.UseRecurringJobs();

        var stored = StoredJobs(host);
        stored.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            [RecurringJobsServiceExtensions.SlaJobId] = Cron.MinuteInterval(15),
            [RecurringJobsServiceExtensions.PostingSweepJobId] = Cron.MinuteInterval(5),
            [RecurringJobsServiceExtensions.IdempotencyCleanupJobId] = Cron.HourInterval(1),
        });
    }

    [HangfireFact]
    [Trait("Rule", "S:JOB-Registration")]
    public async Task UseRecurringJobs_OnARealDatabaseWithTheBlobSweepEnabled_StoresItsJobToo()
    {
        var database = await CreateDatabaseAsync();
        var settings = new Dictionary<string, string?> { [$"{BlobSweepOptions.SectionName}:Enabled"] = "true" };
        var host = database.Build(settings, Mock.Of<MediatR.IMediator>());
        var app = new ApplicationBuilder(host.Services);

        app.UseRecurringJobs();

        StoredJobs(host).Keys.Should().Contain(RecurringJobsServiceExtensions.BlobSweepJobId);
    }

    [HangfireFact]
    [Trait("Rule", "S:JOB-Registration")]
    public async Task UseRecurringJobs_OnARealDatabaseAfterTheBlobSweepIsTurnedOff_RemovesItsStoredJob()
    {
        var database = await CreateDatabaseAsync();
        var enabled = new Dictionary<string, string?> { [$"{BlobSweepOptions.SectionName}:Enabled"] = "true" };
        new ApplicationBuilder(database.Build(enabled, Mock.Of<MediatR.IMediator>()).Services).UseRecurringJobs();
        var host = database.Build(new Dictionary<string, string?>(), Mock.Of<MediatR.IMediator>());
        var app = new ApplicationBuilder(host.Services);

        app.UseRecurringJobs();

        StoredJobs(host).Keys.Should().NotContain(RecurringJobsServiceExtensions.BlobSweepJobId);
    }

    private static Dictionary<string, string> StoredJobs(Microsoft.Extensions.Hosting.IHost host)
    {
        var storage = host.Services.GetRequiredService<JobStorage>();
        using var connection = storage.GetConnection();

        return connection.GetRecurringJobs().ToDictionary(job => job.Id, job => job.Cron);
    }

    private async Task<HangfireTestHost> CreateDatabaseAsync()
    {
        var database = await HangfireTestHost.CreateAsync();
        _databases.Add(database);

        return database;
    }
}
