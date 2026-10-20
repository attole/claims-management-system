using ClaimsModule.Infrastructure.Clock;

namespace ClaimsModule.Infrastructure.Tests.Clock;

public sealed class SystemClockTests
{
    [Fact]
    public void UtcNow_WhenRead_IsInUtcAndBetweenTheTimeBeforeAndAfter()
    {
        var clock = new SystemClock();
        var before = DateTimeOffset.UtcNow;

        var result = clock.UtcNow;

        result.Offset.Should().Be(TimeSpan.Zero);
        result.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTimeOffset.UtcNow);
    }
}
