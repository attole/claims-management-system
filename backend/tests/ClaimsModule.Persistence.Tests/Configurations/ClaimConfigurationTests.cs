using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class ClaimConfigurationTests
{
    [Fact]
    [Trait("Rule", "S:DATA-EfConfig")]
    public void Configure_OpenedClaim_ReadsBackTheRootAndEveryChild()
    {
        using var database = new TestDatabase();
        var written = TestClaims.Opened(database);
        TestClaims.Save(database, written);
        using var context = database.CreateContext();

        var result = context.Claims
            .Include(claim => claim.LossEvent)
            .Include(claim => claim.Parties)
            .Include(claim => claim.RiskObjects)
            .Include(claim => claim.Issues)
            .Include(claim => claim.StateChanges)
            .Single();

        result.ClaimNumber.Should().Be(written.ClaimNumber);
        result.State.Should().Be(new ClaimOpenState());
        result.Severity.Should().Be(ClaimSeverity.Unspecified);
        result.ReportedDate.Should().Be(database.Now);
        result.PolicyId.Should().Be(written.PolicyId);
        result.AssignedHandlerId.Should().Be(TestDatabase.UserId);
        result.LossEvent.Id.Should().Be(written.LossEvent.Id);
        result.Parties.Should().ContainSingle();
        result.RiskObjects.Should().ContainSingle();
        result.Issues.Should().HaveCount(written.Issues.Count);
        result.StateChanges.Should().ContainSingle();
    }

    [Fact]
    [Trait("Rule", "S:BR-C-04")]
    public void Configure_SecondClaimWithTheSameNumber_IsRefused()
    {
        using var database = new TestDatabase();
        TestClaims.Seed(database);
        using var context = database.CreateContext();
        context.Claims.Add(TestClaims.Create(database));

        Func<int> act = () => context.SaveChanges();

        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void Configure_RemovedClaim_IsHiddenFromQueries()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var claim = TestClaims.Create(database);
        context.Claims.Add(claim);
        context.SaveChanges();
        context.Claims.Remove(claim);
        context.SaveChanges();

        var result = context.Claims.ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Configure_PartyAddedToALoadedClaim_IsInserted()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using var context = database.CreateContext();
        var claim = context.Claims.Include(found => found.Parties).Single(found => found.Id == claimId);
        var witness = new PartyDetails(new PartyWitnessRole(), new PartyPersonType()) { FirstName = "Bo" };
        claim.AddParty(TestClaims.PartyInput(database, witness));

        await context.SaveChangesAsync();

        using var reader = database.CreateContext();
        reader.Set<ClaimParty>().Should().ContainSingle(party => party.Details.FirstName == "Bo");
    }

    [Theory]
    [InlineData(ClaimStateKind.Draft, "Draft")]
    [InlineData(ClaimStateKind.Open, "Open")]
    [InlineData(ClaimStateKind.UnderInvestigation, "UnderInvestigation")]
    [InlineData(ClaimStateKind.PendingPayment, "PendingPayment")]
    [InlineData(ClaimStateKind.Closed, "Closed")]
    [InlineData(ClaimStateKind.Reopened, "Reopened")]
    [InlineData(ClaimStateKind.Withdrawn, "Withdrawn")]
    public void Configure_State_IsStoredAsTheEnumMemberName(ClaimStateKind kind, string stored)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Claim))!;
        var converter = entityType.FindProperty(nameof(Claim.State))!.GetValueConverter()!;

        var result = converter.ConvertToProvider(ClaimState.From(kind));

        result.Should().Be(stored);
    }

    [Theory]
    [InlineData(ClaimSeverity.Unspecified, "Unspecified")]
    [InlineData(ClaimSeverity.Minor, "Minor")]
    [InlineData(ClaimSeverity.Standard, "Standard")]
    [InlineData(ClaimSeverity.Critical, "Critical")]
    [InlineData(ClaimSeverity.Catastrophic, "Catastrophic")]
    public void Configure_Severity_IsStoredAsTheEnumMemberName(ClaimSeverity severity, string stored)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Claim))!;
        var converter = entityType.FindProperty(nameof(Claim.Severity))!.GetTypeMapping().Converter!;

        var result = converter.ConvertToProvider(severity);

        result.Should().Be(stored);
    }
}
