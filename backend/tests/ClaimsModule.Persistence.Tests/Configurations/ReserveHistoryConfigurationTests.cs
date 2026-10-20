using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using ClaimsModule.Persistence.Tests.Support.ReserveComponents;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class ReserveHistoryConfigurationTests
{
    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Configure_AutoApprovedRow_ReadsBackEveryValue()
    {
        using var database = new TestDatabase();
        var claimId = TestClaims.Seed(database);
        ReserveHistory written;
        using (var writer = database.CreateContext())
        {
            var component = TestReserves.Open(claimId, TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
            written = TestReserves.Submit(component, TestReserves.AutoApprovedAmount, database.Now);
            writer.ReserveComponents.Add(component);
            writer.SaveChanges();
        }

        using var context = database.CreateContext();

        var result = context.Set<ReserveHistory>().Single(history => history.Id == written.Id);

        result.Details.ClaimId.Should().Be(claimId);
        result.Details.ComponentType.Should().Be(new ReserveIndemnityComponentType());
        result.Details.TransactionType.Should().Be(ReserveTransactionType.Add);
        result.Details.Amount.Should().Be(TestReserves.AutoApprovedAmount);
        result.Details.ChangeSequence.Should().Be(1);
        result.ApprovalState.Should().Be(new ReserveAutoApprovedState());
        result.PostingState.Should().Be(new ReservePostingPendingState());
        result.Balances.IsSet.Should().BeTrue();
        result.Balances.NewBalance.Should().Be(TestReserves.AutoApprovedAmount);
        result.Review.Should().Be(written.Review);
        result.Review.IsSet.Should().BeTrue();
        result.SubmittedAt.Should().Be(database.Now);
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Configure_PendingRow_ReadsBackEmptyBalancesAndReview()
    {
        using var database = new TestDatabase();
        using (var writer = database.CreateContext())
        {
            var component = TestReserves.Open(
                TestClaims.Seed(database),
                TestDatabase.OrganisationId,
                ReserveComponentTypeKind.Indemnity);
            TestReserves.Submit(component, TestReserves.PendingAmount, database.Now);
            writer.ReserveComponents.Add(component);
            writer.SaveChanges();
        }

        using var context = database.CreateContext();

        var result = context.Set<ReserveHistory>().Single();

        result.ApprovalState.Should().Be(new ReservePendingApprovalState());
        result.Balances.IsSet.Should().BeFalse();
        result.Review.IsSet.Should().BeFalse();
        result.PostingDetails.IsSet.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Configure_PostedRow_ReadsBackThePostingJob()
    {
        using var database = new TestDatabase();
        using (var writer = database.CreateContext())
        {
            var component = TestReserves.Open(
                TestClaims.Seed(database),
                TestDatabase.OrganisationId,
                ReserveComponentTypeKind.Indemnity);
            var history = TestReserves.Submit(component, TestReserves.AutoApprovedAmount, database.Now);
            TestReserves.MarkPosted(component, history);
            writer.ReserveComponents.Add(component);
            writer.SaveChanges();
        }

        using var context = database.CreateContext();

        var result = context.Set<ReserveHistory>().Single();

        result.PostingState.Should().Be(new ReservePostingPostedState());
        result.PostingDetails.JobId.Should().Be("job-1");
    }

    [Fact]
    [Trait("Rule", "S:RES-EventSourced")]
    public void Configure_RemovedRow_IsRefused()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var claimId = TestClaims.Seed(database);
        var component = TestReserves.Open(claimId, TestDatabase.OrganisationId, ReserveComponentTypeKind.Indemnity);
        var history = TestReserves.Submit(component, TestReserves.AutoApprovedAmount, database.Now);
        context.ReserveComponents.Add(component);
        context.SaveChanges();
        context.Set<ReserveHistory>().Remove(history);

        Func<int> act = () => context.SaveChanges();

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(ReserveApprovalStateKind.AutoApproved, "AutoApproved")]
    [InlineData(ReserveApprovalStateKind.PendingApproval, "PendingApproval")]
    [InlineData(ReserveApprovalStateKind.Approved, "Approved")]
    [InlineData(ReserveApprovalStateKind.Rejected, "Rejected")]
    [InlineData(ReserveApprovalStateKind.Cancelled, "Cancelled")]
    public void Configure_ApprovalState_IsStoredAsTheEnumMemberName(ReserveApprovalStateKind kind, string stored)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(ReserveHistory))!;
        var converter = entityType.FindProperty(nameof(ReserveHistory.ApprovalState))!.GetValueConverter()!;

        var result = converter.ConvertToProvider(ReserveApprovalState.From(kind));

        result.Should().Be(stored);
    }

    [Theory]
    [InlineData(ReservePostingStateKind.Pending, "Pending")]
    [InlineData(ReservePostingStateKind.Posted, "Posted")]
    [InlineData(ReservePostingStateKind.Failed, "Failed")]
    [InlineData(ReservePostingStateKind.Cancelled, "Cancelled")]
    public void Configure_PostingState_IsStoredAsTheEnumMemberName(ReservePostingStateKind kind, string stored)
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(ReserveHistory))!;
        var converter = entityType.FindProperty(nameof(ReserveHistory.PostingState))!.GetValueConverter()!;

        var result = converter.ConvertToProvider(ReservePostingState.From(kind));

        result.Should().Be(stored);
    }
}
