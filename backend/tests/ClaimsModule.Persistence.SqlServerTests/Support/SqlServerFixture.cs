using ClaimsModule.Application.Abstractions;
using ClaimsModule.Persistence.Interceptors;
using ClaimsModule.Persistence.Schema.Seed;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.SqlServerTests.Support;

public sealed class SqlServerFixture : IDisposable
{
    private const string ServerVariable = "CLAIMS_TEST_SQLSERVER";

    private const string LocalDbServer =
        "Server=(localdb)\\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5";

    private static readonly string _server = Environment.GetEnvironmentVariable(ServerVariable) ?? LocalDbServer;

    private static readonly Lazy<bool> _isAvailable = new(CanConnect);

    private readonly string _databaseName = $"ClaimsModule_Test_{Guid.NewGuid():N}";

    public SqlServerFixture()
    {
        if (IsRequired && !IsAvailable)
        {
            throw new InvalidOperationException($"{ServerVariable} is set but the server does not answer.");
        }

        if (IsAvailable)
        {
            using var context = CreateContext(Guid.NewGuid());
            context.Database.Migrate();
        }
    }

    public static DateTimeOffset Now { get; } = new(2026, 3, 1, 9, 0, 0, TimeSpan.Zero);

    public static bool IsAvailable => _isAvailable.Value;

    public static bool IsRequired => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(ServerVariable));

    public string ConnectionString => $"{_server};Database={_databaseName}";

    public static IClock CreateClock()
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(source => source.UtcNow).Returns(Now);

        return clock.Object;
    }

    public static ICurrentUser CreateUser(Guid organisationId)
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(current => current.Id).Returns(SeedIds.SystemUser);
        user.SetupGet(current => current.OrganisationId).Returns(organisationId);

        return user.Object;
    }

    public ClaimsDbContext CreateContext(Guid organisationId)
    {
        var user = CreateUser(organisationId);
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseSqlServer(ConnectionString)
            .AddInterceptors(SaveInterceptors.Create(CreateClock(), user))
            .Options;

        return new ClaimsDbContext(options, user);
    }

    public void Dispose()
    {
        if (IsAvailable)
        {
            SqlConnection.ClearAllPools();
            using var context = CreateContext(Guid.NewGuid());
            context.Database.EnsureDeleted();
        }
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
