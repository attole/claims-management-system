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
}
