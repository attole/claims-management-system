using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.Reserves;

namespace ClaimsModule.Domain.Tests.ReserveChanges;

public sealed class ReserveChangeDetailsTests
{
    private static readonly ReserveComponentType _indemnity = new ReserveIndemnityComponentType();

    [Theory]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    [InlineData(50_000, 50_000)]
    [InlineData(-50_000, 50_000)]
    public void AbsoluteAmount_IsTheSizeOfTheChange(int amount, int expected)
    {
        var details = CreateDetails(amount);

        details.AbsoluteAmount.Should().Be(expected);
    }

    [Theory]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    [InlineData(10_000, true)]
    [InlineData(-10_000, true)]
    [InlineData(10_001, false)]
    [InlineData(-10_001, false)]
    public void IsWithinAutoApprovalLimit_IncludesTheLimitForEitherSign(int amount, bool isExpected)
    {
        var details = CreateDetails(amount);

        details.IsWithinAutoApprovalLimit.Should().Be(isExpected);
    }

    [Fact]
    public void BalanceAfter_AddsTheSignedAmountToTheApprovedBalance()
    {
        var details = CreateDetails(-500m);

        var result = details.BalanceAfter(2_000m);

        result.Should().Be(1_500m);
    }

    [Theory]
    [Trait("Rule", "S:BR-R-01")]
    [InlineData(500, true)]
    [InlineData(499, false)]
    public void CanHoldBalanceAfter_RefusesANegativeBalanceForIndemnity(int approvedBalance, bool isExpected)
    {
        var details = CreateDetails(-500m);

        details.CanHoldBalanceAfter(approvedBalance).Should().Be(isExpected);
    }

    private static ReserveChangeDetails CreateDetails(decimal amount)
    {
        var details = new ReserveChangeDetails(
            Guid.Empty,
            Guid.Empty,
            _indemnity,
            ReserveTransactionType.Add,
            amount,
            "Reason",
            1);

        return details;
    }
}
