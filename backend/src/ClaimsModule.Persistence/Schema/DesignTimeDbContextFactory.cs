using ClaimsModule.Application.Abstractions;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Persistence.Schema.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClaimsModule.Persistence.Schema;

internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ClaimsDbContext>
{
    private const string ConnectionStringVariable = "CLAIMS_DB_CONNECTION";

    private const string LocalDbConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=ClaimsModule;Trusted_Connection=True;TrustServerCertificate=True";

    public ClaimsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable)
            ?? LocalDbConnectionString;
        var options = new DbContextOptionsBuilder<ClaimsDbContext>().UseSqlServer(connectionString).Options;

        return new ClaimsDbContext(options, new DesignTimeUser());
    }

    private sealed class DesignTimeUser : ICurrentUser
    {
        public Guid Id { get; } = SeedIds.SystemUser;

        public Guid OrganisationId { get; } = SeedIds.Organisation;

        public UserRole Role { get; } = new UserManagerRole();
    }
}
