using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Application.Idempotency;
using ClaimsModule.Persistence.Idempotency;
using ClaimsModule.Persistence.SqlServerTests.Support;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.SqlServerTests;

[Collection(SqlServerCollection.Name)]
public sealed class UnitOfWorkTests(SqlServerFixture database)
{
    [SqlServerFact]
    public async Task CommitAsync_SecondRequestWithTheSameIdempotencyKey_ReportsAConflict()
    {
        var organisationId = Guid.NewGuid();
        var request = new IdempotencyRequest(Guid.NewGuid().ToString("N"), "POST", "/api/claims", "hash");
        using var first = database.CreateContext(organisationId);
        using var second = database.CreateContext(organisationId);
        await CreateStore(first, organisationId).BeginAsync(request, CancellationToken.None);
        await CreateStore(second, organisationId).BeginAsync(request, CancellationToken.None);
        await first.SaveChangesAsync();

        Func<Task> act = () => CreateUnitOfWork(second, organisationId).CommitAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
    }

    [SqlServerFact]
    [Trait("Rule", "S:DATA-RowVer")]
    public async Task CommitAsync_ClaimChangedByAnotherRequest_ReportsAConflict()
    {
        var organisationId = Guid.NewGuid();
        var claim = SqlServerClaims.Create(organisationId);
        using (var setup = database.CreateContext(organisationId))
        {
            setup.Claims.Add(claim);
            await setup.SaveChangesAsync();
        }

        using var context = database.CreateContext(organisationId);
        var loaded = await context.Claims.SingleAsync(saved => saved.Id == claim.Id);
        using (var other = database.CreateContext(organisationId))
        {
            await other.Database.ExecuteSqlAsync($"UPDATE Claims SET Status = Status WHERE ClaimId = {claim.Id}");
        }

        context.Entry(loaded).Property(RowColumns.UpdatedAt).CurrentValue = SqlServerFixture.Now;

        Func<Task> act = () => CreateUnitOfWork(context, organisationId).CommitAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
    }

    private static IdempotencyStore CreateStore(ClaimsDbContext context, Guid organisationId)
    {
        var user = SqlServerFixture.CreateUser(organisationId);

        return new IdempotencyStore(context, SqlServerFixture.CreateClock(), user);
    }

    private static UnitOfWork CreateUnitOfWork(ClaimsDbContext context, Guid organisationId)
    {
        var dispatcher = new Mock<IEventDispatcher>();
        var correlation = new Mock<ICorrelationContext>();

        return new UnitOfWork(
            context,
            dispatcher.Object,
            SqlServerFixture.CreateClock(),
            SqlServerFixture.CreateUser(organisationId),
            correlation.Object);
    }
}
