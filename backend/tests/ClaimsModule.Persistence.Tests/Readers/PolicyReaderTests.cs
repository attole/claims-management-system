using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Paging;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Persistence.Readers;
using ClaimsModule.Persistence.Rows;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Policies;

namespace ClaimsModule.Persistence.Tests.Readers;

public sealed class PolicyReaderTests
{
    private static readonly PageWindow _firstPage = new(1, 10);

    [Fact]
    public async Task ExistsAsync_StoredPolicy_IsTrue()
    {
        using var database = new TestDatabase();
        var policy = Seed(database, "POL-1", null);
        using var context = database.CreateContext();
        var reader = new PolicyReader(context, CreateClock(database));

        var result = await reader.ExistsAsync(policy.Id, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetPeriodAsync_Policy_ReturnsItsDatesWithTheCancellationDate()
    {
        using var database = new TestDatabase();
        var policy = Seed(database, "POL-1", new DateOnly(2026, 6, 30));
        using var context = database.CreateContext();
        var reader = new PolicyReader(context, CreateClock(database));

        var result = await reader.GetPeriodAsync(policy.Id, CancellationToken.None);

        result.Should().Be(new PolicyPeriod(
            policy.Id,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            new DateOnly(2026, 6, 30)));
    }

    [Fact]
    public async Task GetPeriodAsync_PolicyWithoutCancellation_HasAnEmptyCancellationDate()
    {
        using var database = new TestDatabase();
        var policy = Seed(database, "POL-1", null);
        using var context = database.CreateContext();
        var reader = new PolicyReader(context, CreateClock(database));

        var result = await reader.GetPeriodAsync(policy.Id, CancellationToken.None);

        result.IsCancelled.Should().BeFalse();
    }

    [Fact]
    public async Task GetPeriodAsync_UnknownPolicy_IsTheEmptyPeriod()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var reader = new PolicyReader(context, CreateClock(database));

        var result = await reader.GetPeriodAsync(Ids.Next(), CancellationToken.None);

        result.Should().Be(PolicyPeriod.Empty);
    }

    [Fact]
    [Trait("Rule", "S:API-GET-policies-search")]
    public async Task SearchAsync_PartOfThePolicyNumber_ReturnsTheMatchWithItsCoverageTypesAndStatus()
    {
        using var database = new TestDatabase();
        var policy = Seed(database, "POL-100", null);
        Seed(database, "OTHER-1", null);
        AddCoverageType(database, policy.Id, "Theft");
        AddCoverageType(database, policy.Id, "Fire");
        using var context = database.CreateContext();
        var reader = new PolicyReader(context, CreateClock(database));

        var result = await reader.SearchAsync("POL-1", _firstPage, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items[0].Should().BeEquivalentTo(new
        {
            policy.Id,
            PolicyNumber = "POL-100",
            ClientName = "Acme Ltd",
            EffectiveDate = new DateOnly(2026, 1, 1),
            ExpirationDate = new DateOnly(2026, 12, 31),
            EffectiveEndDate = new DateOnly(2026, 12, 31),
            Status = "Active",
            CoverageTypes = new[] { "Fire", "Theft" },
        });
    }

    [Fact]
    [Trait("Rule", "S:API-GET-policies-search")]
    public async Task SearchAsync_PartOfTheClientName_ReturnsTheClientsPolicies()
    {
        using var database = new TestDatabase();
        Seed(database, "POL-1", null);
        using var context = database.CreateContext();
        var reader = new PolicyReader(context, CreateClock(database));

        var result = await reader.SearchAsync("cme", _firstPage, CancellationToken.None);

        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchAsync_PolicyEndedByCancellation_IsCancelledAndEndsOnTheCancellation()
    {
        using var database = new TestDatabase();
        Seed(database, "POL-1", new DateOnly(2026, 2, 1));
        using var context = database.CreateContext();
        var reader = new PolicyReader(context, CreateClock(database));

        var result = await reader.SearchAsync("POL", _firstPage, CancellationToken.None);

        result.Items.Should().ContainSingle(item => item.Status == "Cancelled"
            && item.EffectiveEndDate == new DateOnly(2026, 2, 1));
    }

    private static Policy Seed(TestDatabase database, string policyNumber, DateOnly? cancelledDate)
    {
        using var context = database.CreateContext();
        var policy = TestPolicies.Add(context, TestDatabase.OrganisationId, policyNumber, cancelledDate);
        context.SaveChanges();

        return policy;
    }

    private static void AddCoverageType(TestDatabase database, Guid policyId, string name)
    {
        using var context = database.CreateContext();
        var organisationId = TestDatabase.OrganisationId;
        context.PolicyCoverageTypes.Add(new PolicyCoverageType(Ids.Next(), policyId, organisationId, name));
        context.SaveChanges();
    }

    private static IClock CreateClock(TestDatabase database)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(source => source.UtcNow).Returns(database.Now);

        return clock.Object;
    }
}
