using ClaimsModule.Application.Abstractions;
using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Interceptors;
using ClaimsModule.Persistence.Tests.Support.Conventions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Support;

public sealed class TestDatabase : IDisposable
{
    public static readonly Guid OrganisationId = Ids.Next();

    public static readonly Guid UserId = Ids.Next();

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public TestDatabase()
    {
        _connection.Open();
    }

    public DateTimeOffset Now { get; set; } = new(2026, 3, 1, 9, 0, 0, TimeSpan.Zero);

    public ClaimsDbContext CreateContext()
    {
        return CreateContext(OrganisationId, UserId);
    }

    public ClaimsDbContext CreateContext(Guid organisationId, Guid userId)
    {
        var user = CreateUser(organisationId, userId);
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(CreateInterceptor(user))
            .Options;
        var context = new ClaimsDbContext(options, user);
        context.Database.EnsureCreated();

        return context;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    internal ConventionContext CreateConventionContext()
    {
        var user = CreateUser(OrganisationId, UserId);
        var options = new DbContextOptionsBuilder<ConventionContext>()
            .UseSqlite(_connection)
            .AddInterceptors(CreateInterceptor(user))
            .Options;
        var context = new ConventionContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    private static ICurrentUser CreateUser(Guid organisationId, Guid userId)
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(current => current.Id).Returns(userId);
        user.SetupGet(current => current.OrganisationId).Returns(organisationId);

        return user.Object;
    }

    private AuditSoftDeleteInterceptor CreateInterceptor(ICurrentUser user)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(source => source.UtcNow).Returns(() => Now);

        return new AuditSoftDeleteInterceptor(clock.Object, user);
    }
}
