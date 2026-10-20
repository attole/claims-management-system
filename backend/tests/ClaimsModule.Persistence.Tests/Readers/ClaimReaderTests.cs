using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Paging;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Persistence.Readers;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using ClaimsModule.Persistence.Tests.Support.Mapping;
using ClaimsModule.Persistence.Tests.Support.Policies;
using ClaimsModule.Persistence.Tests.Support.ReserveComponents;

namespace ClaimsModule.Persistence.Tests.Readers;

public sealed class ClaimReaderTests
{
    private static readonly PageWindow _firstPage = new(1, 10);

    [Fact]
    public async Task ExistsAsync_StoredClaim_IsTrue()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());

        var result = await reader.ExistsAsync(claimId, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_UnknownClaim_IsFalse()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());

        var result = await reader.ExistsAsync(Ids.Next(), CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task ListAsync_NoFilter_ReturnsTheNewestNumberFirstWithPolicyNamesAndReserveTotals()
    {
        using var database = new TestDatabase();
        var policyId = SeedPolicy(database);
        TestClaims.Save(database, TestClaims.Ready(database, policyId, 1));
        var second = TestClaims.Ready(database, policyId, 2);
        TestClaims.Save(database, second);
        SeedReserve(database, second.Id);
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());

        var result = await reader.ListAsync(new ClaimListFilter(), _firstPage, CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Select(item => item.ClaimNumber).Should().Equal("CLM-2026-0000002", "CLM-2026-0000001");
        result.Items[0].Should().BeEquivalentTo(new
        {
            Id = second.Id,
            ClientName = "Acme Ltd",
            PolicyNumber = "POL-1",
            LossDate = database.Now.AddDays(-3),
            Status = "Draft",
            TotalReserves = TestReserves.AutoApprovedAmount,
        });
        result.Items[1].TotalReserves.Should().Be(0m);
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public async Task ListAsync_RemovedClaim_IsHidden()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var claim = TestClaims.Create(database);
        context.Claims.Add(claim);
        context.SaveChanges();
        context.Claims.Remove(claim);
        context.SaveChanges();
        var reader = new ClaimReader(context, TestMapper.Create());

        var result = await reader.ListAsync(new ClaimListFilter(), _firstPage, CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task ListAsync_StatusFilter_ReturnsOnlyClaimsInThatStatus()
    {
        using var database = new TestDatabase();
        TestClaims.Save(database, TestClaims.Ready(database, Ids.Next(), 1));
        var opened = TestClaims.Opened(database, 2);
        TestClaims.Save(database, opened);
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());
        var filter = new ClaimListFilter { Statuses = [ClaimStateKind.Open] };

        var result = await reader.ListAsync(filter, _firstPage, CancellationToken.None);

        result.Items.Select(item => item.Id).Should().Equal(opened.Id);
    }

    [Fact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task ListAsync_SearchByPartOfTheClaimNumber_ReturnsTheMatch()
    {
        using var database = new TestDatabase();
        var policyId = SeedPolicy(database);
        TestClaims.Save(database, TestClaims.Ready(database, policyId, 1));
        var second = TestClaims.Ready(database, policyId, 2);
        TestClaims.Save(database, second);
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());
        var filter = new ClaimListFilter { Search = "0000002" };

        var result = await reader.ListAsync(filter, _firstPage, CancellationToken.None);

        result.Items.Select(item => item.Id).Should().Equal(second.Id);
    }

    [Fact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task ListAsync_SearchByPartOfTheClientName_ReturnsTheClaimsOfThatClient()
    {
        using var database = new TestDatabase();
        var policyId = SeedPolicy(database);
        TestClaims.Save(database, TestClaims.Ready(database, policyId, 1));
        TestClaims.Save(database, TestClaims.Ready(database, Ids.Next(), 2));
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());
        var filter = new ClaimListFilter { Search = "Acme" };

        var result = await reader.ListAsync(filter, _firstPage, CancellationToken.None);

        result.Items.Should().ContainSingle(item => item.PolicyNumber == "POL-1");
    }

    [Fact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task ListAsync_PolicyFilter_ReturnsOnlyThatPolicysClaims()
    {
        using var database = new TestDatabase();
        var policyId = Ids.Next();
        var first = TestClaims.Ready(database, policyId, 1);
        TestClaims.Save(database, first);
        TestClaims.Save(database, TestClaims.Ready(database, Ids.Next(), 2));
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());
        var filter = new ClaimListFilter { PolicyId = policyId };

        var result = await reader.ListAsync(filter, _firstPage, CancellationToken.None);

        result.Items.Select(item => item.Id).Should().Equal(first.Id);
    }

    [Fact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task ListAsync_HandlerAndCauseFilters_ReturnOnlyMatchingClaims()
    {
        using var database = new TestDatabase();
        TestClaims.Save(database, TestClaims.Ready(database, Ids.Next(), 1));
        TestClaims.Save(database, TestClaims.Create(database, 2));
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());
        var filter = new ClaimListFilter
        {
            AssignedHandlerId = TestDatabase.UserId,
            CauseOfLossCode = TestClaims.CauseCode,
        };

        var result = await reader.ListAsync(filter, _firstPage, CancellationToken.None);

        result.Items.Should().ContainSingle(item => item.ClaimNumber == "CLM-2026-0000001");
    }

    [Fact]
    public async Task ListAsync_SecondPage_ReturnsTheRemainingClaimsAndTheTotal()
    {
        using var database = new TestDatabase();
        TestClaims.Seed(database, 1);
        TestClaims.Seed(database, 2);
        TestClaims.Seed(database, 3);
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());

        var result = await reader.ListAsync(new ClaimListFilter(), new PageWindow(2, 2), CancellationToken.None);

        result.Items.Should().ContainSingle(item => item.ClaimNumber == "CLM-2026-0000001");
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task ListAsync_ClaimOfAnotherOrganisation_IsHidden()
    {
        using var database = new TestDatabase();
        TestClaims.Seed(database);
        using var context = database.CreateContext(Ids.Next(), TestDatabase.UserId);
        var reader = new ClaimReader(context, TestMapper.Create());

        var result = await reader.ListAsync(new ClaimListFilter(), _firstPage, CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:API-GET-claim")]
    public async Task GetDetailAsync_Claim_ReturnsTheClaimLossPartiesRiskObjectsAndPolicyNames()
    {
        using var database = new TestDatabase();
        var written = TestClaims.Ready(database, SeedPolicy(database));
        TestClaims.Save(database, written);
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());

        var result = await reader.GetDetailAsync(written.Id, CancellationToken.None);

        result.Value.Claim.ClaimNumber.Should().Be(written.ClaimNumber.Value);
        result.Value.ClientName.Should().Be("Acme Ltd");
        result.Value.PolicyNumber.Should().Be("POL-1");
        result.Value.Loss.Description.Should().Be("Water damage in the kitchen");
        result.Value.Loss.EstimatedLossAmount.Should().Be(TestClaims.EstimatedAmount);
        result.Value.Parties.Should().ContainSingle(party => party.Role == "Claimant");
        result.Value.RiskObjects.Should().ContainSingle(riskObject => riskObject.AssetType == "Vehicle");
    }

    [Fact]
    public async Task GetDetailAsync_ClearedIssue_IsNotListed()
    {
        using var database = new TestDatabase();
        var written = TestClaims.Ready(database, Ids.Next());
        TestClaims.Save(database, written);
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());

        var result = await reader.GetDetailAsync(written.Id, CancellationToken.None);

        written.Issues.Should().Contain(issue => !issue.State.IsCurrent);
        result.Value.Issues.Should().NotContain(issue => issue.State == "Cleared");
    }

    [Fact]
    public async Task GetDetailAsync_UnknownClaim_IsNotFound()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var reader = new ClaimReader(context, TestMapper.Create());

        var result = await reader.GetDetailAsync(Ids.Next(), CancellationToken.None);

        result.Errors.Should().ContainSingle(error => error.Code == ClaimErrors.ClaimNotFoundCode);
    }

    private static Guid SeedPolicy(TestDatabase database)
    {
        using var context = database.CreateContext();
        var policy = TestPolicies.Add(context, TestDatabase.OrganisationId, "POL-1", null);
        context.SaveChanges();

        return policy.Id;
    }

    private static void SeedReserve(TestDatabase database, Guid claimId)
    {
        using var context = database.CreateContext();
        var component = TestReserves.Open(claimId, TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
        TestReserves.Submit(component, TestReserves.AutoApprovedAmount, database.Now);
        context.ReserveComponents.Add(component);
        context.SaveChanges();
    }
}
