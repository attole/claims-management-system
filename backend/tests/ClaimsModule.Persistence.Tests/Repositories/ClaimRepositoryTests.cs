using ClaimsModule.Application.Claims;
using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Repositories;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Repositories;

public sealed class ClaimRepositoryTests
{
    [Fact]
    public async Task AddAsync_Claim_IsTrackedAsAddedWithItsChildren()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var repository = new ClaimRepository(context);
        var claim = TestClaims.Opened(database);

        await repository.AddAsync(claim, CancellationToken.None);

        context.Entry(claim).State.Should().Be(EntityState.Added);
        context.Entry(claim.Parties[0]).State.Should().Be(EntityState.Added);
        context.Entry(claim.LossEvent).State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task GetAsync_Claim_ReturnsItWithEveryChild()
    {
        using var database = new TestDatabase();
        var written = TestClaims.Opened(database);
        TestClaims.Save(database, written);
        using var context = database.CreateContext();
        var repository = new ClaimRepository(context);

        var result = await repository.GetAsync(written.Id, CancellationToken.None);

        result.Value.LossEvent.Id.Should().Be(written.LossEvent.Id);
        result.Value.Parties.Should().ContainSingle();
        result.Value.RiskObjects.Should().ContainSingle();
        result.Value.Issues.Should().HaveCount(written.Issues.Count);
        result.Value.StateChanges.Should().ContainSingle();
    }

    [Fact]
    public async Task GetAsync_UnknownClaim_IsNotFound()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var repository = new ClaimRepository(context);

        var result = await repository.GetAsync(Ids.Next(), CancellationToken.None);

        result.Errors.Should().ContainSingle(error => error.Code == ClaimErrors.ClaimNotFoundCode);
    }

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task GetAsync_ClaimOfAnotherOrganisation_IsNotFound()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using var context = database.CreateContext(Ids.Next(), TestDatabase.UserId);
        var repository = new ClaimRepository(context);

        var result = await repository.GetAsync(claimId, CancellationToken.None);

        result.Errors.Should().ContainSingle(error => error.Code == ClaimErrors.ClaimNotFoundCode);
    }
}
