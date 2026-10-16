using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss;

public sealed class LossDateTests
{
    [Fact]
    public void Empty_IsNotSet()
    {
        LossDate.Empty.IsSet.Should().BeFalse();
    }

    [Fact]
    public void ADate_IsSet()
    {
        new LossDate(TestClaims.Start).IsSet.Should().BeTrue();
    }

    [Fact]
    public void Dates_WithTheSameValueAreEqual()
    {
        new LossDate(TestClaims.Start).Should().Be(new LossDate(TestClaims.Start));
    }

    [Fact]
    public void UtcDay_ForADateInAnotherOffset_IsItsUtcDay()
    {
        var lossDate = new LossDate(new DateTimeOffset(2026, 1, 1, 0, 30, 0, TimeSpan.FromHours(2)));

        lossDate.UtcDay.Should().Be(new DateOnly(2025, 12, 31));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(5, false)]
    [InlineData(24, true)]
    public void IsAfterDay_ComparesCalendarDaysOnly(int hoursAfterStart, bool isExpected)
    {
        var lossDate = new LossDate(TestClaims.Start.AddHours(hoursAfterStart));

        lossDate.IsAfterDay(TestClaims.ReportDay).Should().Be(isExpected);
    }

    [Fact]
    public void IsAfterDay_WithoutADate_IsFalse()
    {
        LossDate.Empty.IsAfterDay(DateOnly.MinValue).Should().BeFalse();
    }
}
