using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Policies;

public sealed class PolicyPeriodTests
{
    private static readonly PolicyPeriod _period = new(
        TestClaims.Policy,
        new DateOnly(2026, 1, 1),
        new DateOnly(2026, 12, 31),
        DateOnly.MinValue);

    [Theory]
    [Trait("Rule", "S:BR-C-02")]
    [InlineData(2026, 1, 1)]
    [InlineData(2026, 6, 15)]
    [InlineData(2026, 12, 31)]
    public void CanCover_ADayInsideTheInclusivePeriod_IsTrue(int year, int month, int day)
    {
        var result = _period.CanCover(new DateOnly(year, month, day));

        result.Should().BeTrue();
    }

    [Theory]
    [Trait("Rule", "S:BR-C-02")]
    [InlineData(2025, 12, 31)]
    [InlineData(2027, 1, 1)]
    public void CanCover_ADayOutsideThePeriod_IsFalse(int year, int month, int day)
    {
        var result = _period.CanCover(new DateOnly(year, month, day));

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:BR-C-02")]
    public void CanCover_ADayAfterAnEarlierCancellation_IsFalse()
    {
        var cancelled = _period with { CancelledDate = new DateOnly(2026, 6, 30) };

        var result = cancelled.CanCover(new DateOnly(2026, 7, 1));

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:BR-C-02")]
    public void CanCover_ADayAfterALaterCancellation_StillEndsAtTheExpiration()
    {
        var cancelled = _period with { CancelledDate = new DateOnly(2027, 3, 1) };

        var result = cancelled.CanCover(new DateOnly(2027, 1, 1));

        result.Should().BeFalse();
    }

    [Fact]
    public void Empty_IsNotSet()
    {
        PolicyPeriod.Empty.IsSet.Should().BeFalse();
    }

    [Fact]
    public void RequireFor_ThePeriodsOwnPolicy_ReturnsThePeriod()
    {
        var result = _period.RequireFor(TestClaims.Policy);

        result.Should().BeSameAs(_period);
    }

    [Fact]
    public void RequireFor_AnotherPolicy_Throws()
    {
        Func<PolicyPeriod> act = () => _period.RequireFor(Guid.Empty);

        act.Should().Throw<InvalidOperationException>();
    }
}
