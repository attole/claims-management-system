using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class ClaimStateChangeConfigurationTests
{
    [Fact]
    [Trait("Rule", "S:ST-Draft-Open")]
    public void Configure_StateChange_ReadsBackTheMoveReasonAndNote()
    {
        using var database = new TestDatabase();
        var written = TestClaims.Opened(database);
        TestClaims.Save(database, written);
        using var context = database.CreateContext();

        var result = context.Set<ClaimStateChange>().Single();

        result.ClaimId.Should().Be(written.Id);
        result.From.Should().Be(new ClaimDraftState());
        result.To.Should().Be(new ClaimOpenState());
        result.Reason.Should().Be(StateChangeReason.Empty);
        result.Note.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:DATA-SoftDelete")]
    public void Configure_RemovedStateChange_IsRefused()
    {
        using var database = new TestDatabase();
        TestClaims.Save(database, TestClaims.Opened(database));
        using var context = database.CreateContext();
        var stored = context.Claims.Include(claim => claim.StateChanges).Single();
        context.Remove(stored.StateChanges[0]);

        Func<int> act = () => context.SaveChanges();

        act.Should().Throw<InvalidOperationException>();
    }
}
