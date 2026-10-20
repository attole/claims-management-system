using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Persistence.Repositories;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using ClaimsModule.Persistence.Tests.Support.ReserveComponents;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Repositories;

public sealed class ReserveComponentRepositoryTests
{
    [Fact]
    public async Task AddAsync_Component_IsTrackedAsAdded()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var repository = new ReserveComponentRepository(context);
        var component = TestReserves.Open(Ids.Next(), TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);

        await repository.AddAsync(component, CancellationToken.None);

        context.Entry(component).State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task GetAsync_ComponentOfTheClaim_ReturnsItWithItsHistory()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        var component = await SeedAsync(database, claimId);
        using var context = database.CreateContext();
        var repository = new ReserveComponentRepository(context);

        var result = await repository.GetAsync(claimId, component.Id, CancellationToken.None);

        result.Value.History.Should().ContainSingle();
    }

    [Fact]
    public async Task GetAsync_ComponentOfAnotherClaim_IsNotFound()
    {
        using var database = new TestDatabase();
        var component = await SeedAsync(database, TestClaims.Seed(database));
        using var context = database.CreateContext();
        var repository = new ReserveComponentRepository(context);

        var result = await repository.GetAsync(Ids.Next(), component.Id, CancellationToken.None);

        result.Errors.Should().ContainSingle(error => error.Code == ReserveErrors.ComponentNotFoundCode);
    }

    [Fact]
    public async Task GetByHistoryAsync_RowOfTheClaim_ReturnsItsComponent()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        var component = await SeedAsync(database, claimId);
        using var context = database.CreateContext();
        var repository = new ReserveComponentRepository(context);

        var result = await repository.GetByHistoryAsync(claimId, component.History[0].Id, CancellationToken.None);

        result.Value.Id.Should().Be(component.Id);
    }

    [Fact]
    public async Task GetByHistoryAsync_RowOfAnotherClaim_IsNotFound()
    {
        using var database = new TestDatabase();
        var component = await SeedAsync(database, TestClaims.Seed(database));
        using var context = database.CreateContext();
        var repository = new ReserveComponentRepository(context);

        var result = await repository.GetByHistoryAsync(Ids.Next(), component.History[0].Id, CancellationToken.None);

        result.Errors.Should().ContainSingle(error => error.Code == ReserveErrors.HistoryNotFoundCode);
    }

    [Fact]
    public async Task ListByClaimAsync_Claim_ReturnsOnlyItsComponents()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        await SeedAsync(database, claimId);
        await SeedAsync(database, TestClaims.Seed(database, 2));
        using var context = database.CreateContext();
        var repository = new ReserveComponentRepository(context);

        var result = await repository.ListByClaimAsync(claimId, CancellationToken.None);

        result.Should().ContainSingle();
    }

    private static async Task<ReserveComponent> SeedAsync(TestDatabase database, Guid claimId)
    {
        using var context = database.CreateContext();
        var component = TestReserves.Open(claimId, TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
        TestReserves.Submit(component, TestReserves.AutoApprovedAmount, database.Now);
        context.ReserveComponents.Add(component);
        await context.SaveChangesAsync();

        return component;
    }
}
