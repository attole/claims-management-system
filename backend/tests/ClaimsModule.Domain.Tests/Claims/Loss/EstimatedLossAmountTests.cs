using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Tests.Claims.Loss;

public sealed class EstimatedLossAmountTests
{
    [Fact]
    public void Amount_KeepsItsValueAndIsSet()
    {
        var amount = new EstimatedLossAmount(12.5m);

        amount.Value.Should().Be(12.5m);
        amount.IsSet.Should().BeTrue();
    }

    [Fact]
    public void Empty_IsNotSet()
    {
        EstimatedLossAmount.Empty.IsSet.Should().BeFalse();
    }

    [Fact]
    public void AnEstimateOfZero_IsSetAndDiffersFromEmpty()
    {
        var zero = new EstimatedLossAmount(0m);

        zero.IsSet.Should().BeTrue();
        zero.Should().NotBe(EstimatedLossAmount.Empty);
    }

    [Fact]
    public void Amounts_WithTheSameValueAreEqual()
    {
        new EstimatedLossAmount(12.5m).Should().Be(new EstimatedLossAmount(12.5m));
    }

    [Fact]
    public void Amount_WhenNegative_IsRefused()
    {
        Func<EstimatedLossAmount> act = () => new EstimatedLossAmount(-0.01m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
