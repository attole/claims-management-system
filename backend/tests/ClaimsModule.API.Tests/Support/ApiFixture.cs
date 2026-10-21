using ClaimsModule.Application.Abstractions;
using ClaimsModule.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.API.Tests.Support;

public sealed class ApiFixture : IDisposable
{
    private const string ServerVariable = "CLAIMS_TEST_SQLSERVER";

    private const string LocalDbServer =
        "Server=(localdb)\\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5";

    private static readonly string _server = Environment.GetEnvironmentVariable(ServerVariable) ?? LocalDbServer;

    private static readonly Lazy<bool> _isAvailable = new(CanConnect);

    private readonly string _databaseName = $"ClaimsModule_ApiTest_{Guid.NewGuid():N}";

    public ApiFixture()
    {
        if (IsAvailable)
        {
            using var context = CreateContext();
            context.Database.Migrate();
        }
    }

    public static bool IsAvailable => _isAvailable.Value;

    public string ConnectionString => $"{_server};Database={_databaseName}";

    public void Dispose()
    {
        if (IsAvailable)
        {
            SqlConnection.ClearAllPools();
            using var context = CreateContext();
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

    private ClaimsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ClaimsDbContext>().UseSqlServer(ConnectionString).Options;

        return new ClaimsDbContext(options, new Mock<ICurrentUser>().Object);
    }
}
