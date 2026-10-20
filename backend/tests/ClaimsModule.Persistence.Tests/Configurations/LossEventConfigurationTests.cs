using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Claims;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class LossEventConfigurationTests
{
    [Fact]
    [Trait("Rule", "S:DATA-EfConfig")]
    public void Configure_LossDetails_ReadBackEveryValue()
    {
        using var database = new TestDatabase();
        var written = TestClaims.Ready(database, Ids.Next());
        TestClaims.Save(database, written);
        using var context = database.CreateContext();

        var result = context.Set<LossEvent>().Single();

        result.ClaimId.Should().Be(written.Id);
        result.ReportDate.Should().Be(written.LossEvent.ReportDate);
        result.Details.Should().Be(written.LossEvent.Details);
    }

    [Fact]
    [Trait("Rule", "S:BR-C-05")]
    public void Configure_DraftWithoutLoss_ReadsBackAnEmptyLossDateAndEstimate()
    {
        using var database = new TestDatabase();
        TestClaims.Seed(database);
        using var context = database.CreateContext();

        var result = context.Set<LossEvent>().Single();

        result.Details.LossDate.IsSet.Should().BeFalse();
        result.Details.EstimatedLossAmount.IsSet.Should().BeFalse();
    }

    [Fact]
    public void Configure_ZeroEstimate_ReadsBackAsASetEstimate()
    {
        using var database = new TestDatabase();
        var claim = TestClaims.Create(database);
        var loss = new LossDetails(LossDate.Empty, "Water damage in the kitchen", TestClaims.CauseCode)
        {
            EstimatedLossAmount = new EstimatedLossAmount(0m),
        };
        claim.UpdateLossDetails(TestClaims.LossInput(database, loss));
        TestClaims.Save(database, claim);
        using var context = database.CreateContext();

        var result = context.Set<LossEvent>().Single();

        result.Details.EstimatedLossAmount.IsSet.Should().BeTrue();
    }
}
