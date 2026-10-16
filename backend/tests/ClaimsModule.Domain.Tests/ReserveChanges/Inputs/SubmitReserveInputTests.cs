using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Inputs;

public sealed class SubmitReserveInputTests
{
    [Fact]
    public void Now_WhenNotGiven_Throws()
    {
        Func<SubmitReserveInput> act = () => new SubmitReserveInput(
            TestReserves.Reference(),
            ReserveTransactionType.Add,
            1_000m,
            "Initial estimate",
            TestReserves.Submitter,
            true,
            true,
            0m,
            0m,
            1,
            default);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReserveBalanceAfter_AddsTheAmountToTheApprovedBalance()
    {
        var input = TestReserves.SubmitInput(-2_000m) with { ReserveApprovedBalance = 5_000m };

        input.ReserveBalanceAfter.Should().Be(3_000m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeSequence_WhenNotPositive_Throws(int changeSequence)
    {
        Func<SubmitReserveInput> act = () => TestReserves.SubmitInput(1_000m, changeSequence);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ActorId_WhenNotGiven_Throws()
    {
        Func<SubmitReserveInput> act = () => new SubmitReserveInput(
            TestReserves.Reference(),
            ReserveTransactionType.Add,
            1_000m,
            "Initial estimate",
            Guid.Empty,
            true,
            true,
            0m,
            0m,
            1,
            TestClaims.Start);

        act.Should().Throw<ArgumentException>();
    }
}
