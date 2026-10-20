using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Persistence.Readers;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using ClaimsModule.Persistence.Tests.Support.Mapping;
using ClaimsModule.Persistence.Tests.Support.ReserveComponents;

namespace ClaimsModule.Persistence.Tests.Readers;

public sealed class ClaimReserveReaderTests
{
    [Fact]
    public async Task GetFactsAsync_ClaimWithoutReserves_HasNoFacts()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var reader = new ClaimReserveReader(context, TestMapper.Create());

        var result = await reader.GetFactsAsync(Ids.Next(), CancellationToken.None);

        result.HasPendingHistory.Should().BeFalse();
        result.HasUnpostedHistory.Should().BeFalse();
        result.HasOpenBalance.Should().BeFalse();
        result.HasApprovedReserve.Should().BeFalse();
        result.ApprovedTotal.Should().Be(0m);
    }

    [Fact]
    public async Task GetFactsAsync_PendingRow_IsPendingButNotApproved()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        Seed(database, claimId, TestReserves.PendingAmount);
        using var context = database.CreateContext();
        var reader = new ClaimReserveReader(context, TestMapper.Create());

        var result = await reader.GetFactsAsync(claimId, CancellationToken.None);

        result.HasPendingHistory.Should().BeTrue();
        result.HasApprovedReserve.Should().BeFalse();
        result.HasOpenBalance.Should().BeFalse();
    }

    [Fact]
    public async Task GetFactsAsync_AutoApprovedRowNotPosted_IsApprovedUnpostedWithABalance()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        Seed(database, claimId, TestReserves.AutoApprovedAmount);
        using var context = database.CreateContext();
        var reader = new ClaimReserveReader(context, TestMapper.Create());

        var result = await reader.GetFactsAsync(claimId, CancellationToken.None);

        result.HasUnpostedHistory.Should().BeTrue();
        result.HasApprovedReserve.Should().BeTrue();
        result.HasOpenBalance.Should().BeTrue();
        result.ApprovedTotal.Should().Be(TestReserves.AutoApprovedAmount);
    }

    [Fact]
    public async Task GetFactsAsync_PostedRow_IsNotUnposted()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        Seed(database, claimId, TestReserves.AutoApprovedAmount, isPosted: true);
        using var context = database.CreateContext();
        var reader = new ClaimReserveReader(context, TestMapper.Create());

        var result = await reader.GetFactsAsync(claimId, CancellationToken.None);

        result.HasUnpostedHistory.Should().BeFalse();
    }

    [Fact]
    public async Task GetReservesAsync_Claim_ReturnsTheComponentsHistoryAndTheApprovedTotal()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        Seed(database, claimId, TestReserves.AutoApprovedAmount);
        using var context = database.CreateContext();
        var reader = new ClaimReserveReader(context, TestMapper.Create());

        var result = await reader.GetReservesAsync(claimId, CancellationToken.None);

        result.ApprovedTotal.Should().Be(TestReserves.AutoApprovedAmount);
        result.Components.Should().ContainSingle(component => component.ComponentType == "Indemnity"
            && component.State == "Active"
            && component.ApprovedBalance == TestReserves.AutoApprovedAmount);
        result.History.Should().ContainSingle(row => row.ApprovalStatus == "AutoApproved"
            && row.PostingStatus == "Pending"
            && row.IdempotencyKey.StartsWith("Reserve:", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ListStalePendingPostingsAsync_ApprovedRowOlderThanTheLimit_IsListedAcrossOrganisations()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        var history = Seed(database, claimId, TestReserves.AutoApprovedAmount);
        Seed(database, TestClaims.Seed(database, 2), TestReserves.PendingAmount);
        Seed(database, TestClaims.Seed(database, 3), TestReserves.AutoApprovedAmount, isPosted: true);
        using var context = database.CreateContext(Guid.Empty, Guid.Empty);
        var reader = new ClaimReserveReader(context, TestMapper.Create());

        var result = await reader.ListStalePendingPostingsAsync(database.Now.AddHours(1), CancellationToken.None);

        result.Should().ContainSingle(row => row.ClaimId == claimId && row.ReserveHistoryId == history);
    }

    [Fact]
    public async Task ListStalePendingPostingsAsync_ApprovedRowNewerThanTheLimit_IsNotListed()
    {
        using var database = new TestDatabase();
        Seed(database, TestClaims.Seed(database), TestReserves.AutoApprovedAmount);
        using var context = database.CreateContext(Guid.Empty, Guid.Empty);
        var reader = new ClaimReserveReader(context, TestMapper.Create());

        var result = await reader.ListStalePendingPostingsAsync(database.Now.AddHours(-1), CancellationToken.None);

        result.Should().BeEmpty();
    }

    private static Guid Seed(TestDatabase database, Guid claimId, decimal amount, bool isPosted = false)
    {
        using var context = database.CreateContext();
        var component = TestReserves.Open(claimId, TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
        var history = TestReserves.Submit(component, amount, database.Now);

        if (isPosted)
        {
            TestReserves.MarkPosted(component, history);
        }

        context.ReserveComponents.Add(component);
        context.SaveChanges();

        return history.Id;
    }
}
