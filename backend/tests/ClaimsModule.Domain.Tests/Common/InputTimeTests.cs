using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Common;

public sealed class InputTimeTests
{
    [Fact]
    public void Required_WithATime_ReturnsIt()
    {
        var result = InputTime.Required(TestClaims.Start);

        result.Should().Be(TestClaims.Start);
    }

    [Fact]
    public void Required_WithoutATime_Throws()
    {
        Func<DateTimeOffset> act = () => InputTime.Required(DateTimeOffset.MinValue);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NotBefore_ATimeAfterTheEarliest_ReturnsIt()
    {
        var result = InputTime.NotBefore(TestClaims.Later, TestClaims.Start);

        result.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void NotBefore_ATimeBeforeTheEarliest_Throws()
    {
        Func<DateTimeOffset> act = () => InputTime.NotBefore(TestClaims.Start, TestClaims.Later);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
