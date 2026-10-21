using ClaimsModule.Infrastructure.BackgroundJobs;
using ClaimsModule.Infrastructure.BackgroundJobs.Recurring;
using ClaimsModule.Infrastructure.Identity;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClaimsModule.Infrastructure.Tests.Support.BackgroundJobs;

public sealed class HangfireTestHost : IAsyncDisposable
{
    private const string ServerVariable = "CLAIMS_TEST_SQLSERVER";

    private const string LocalDbServer =
        "Server=(localdb)\\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5";

    private static readonly string _server = Environment.GetEnvironmentVariable(ServerVariable) ?? LocalDbServer;

    private static readonly Lazy<bool> _isAvailable = new(CanConnect);

    private readonly string _databaseName = $"ClaimsModule_HangfireTest_{Guid.NewGuid():N}";

    private readonly List<IHost> _hosts = [];

    public static bool IsAvailable => _isAvailable.Value;

    public static bool IsRequired => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(ServerVariable));

    private string ConnectionString => $"{_server};Database={_databaseName}";

    public static async Task<HangfireTestHost> CreateAsync()
    {
        if (IsRequired && !IsAvailable)
        {
            throw new InvalidOperationException($"{ServerVariable} is set but the server does not answer.");
        }

        var host = new HangfireTestHost();
        await using var connection = new SqlConnection(_server);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"CREATE DATABASE [{host._databaseName}]", connection);
        await command.ExecuteNonQueryAsync();

        return host;
    }

    public IHost Build(IDictionary<string, string?> settings, IMediator mediator)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(settings);
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:ClaimsDb"] = ConnectionString,
        });
        builder.Services.AddBackgroundJobs();
        builder.Services.AddRecurringJobs();
        builder.Services.AddSingleton(mediator);
        builder.Services.AddScoped<ScopedCurrentUser>();

        var host = builder.Build();
        _hosts.Add(host);

        return host;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var host in _hosts)
        {
            await host.StopAsync();
            host.Dispose();
        }

        SqlConnection.ClearAllPools();
        await using var connection = new SqlConnection(_server);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE [{_databaseName}]",
            connection);
        await command.ExecuteNonQueryAsync();
    }

    private static bool CanConnect()
    {
        try
        {
            using var connection = new SqlConnection(_server);
            connection.Open();

            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }
}
