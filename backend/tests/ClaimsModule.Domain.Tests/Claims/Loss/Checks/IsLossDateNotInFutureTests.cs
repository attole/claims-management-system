using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Checks;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Checks;

public sealed class IsLossDateNotInFutureTests
{
    [Fact]
    [Trait("Rule", IsLossDateNotInFuture.Code)]
    public void Check_FailsWhenTheLossDateIsAfterNow()
    {
        var claim = TestClaims.WithLoss(TestClaims.Later.AddTicks(1));

        var result = new IsLossDateNotInFuture(TestClaims.Later).Check(claim.LossEvent.Details);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsLossDateNotInFuture.Code);
    }

    [Fact]
    [Trait("Rule", IsLossDateNotInFuture.Code)]
    public void Check_PassesWhenTheLossDateIsNow()
    {
        var claim = TestClaims.WithLoss(TestClaims.Later);

        var result = new IsLossDateNotInFuture(TestClaims.Later).Check(claim.LossEvent.Details);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossDateNotInFuture.Code)]
    public void Check_PassesWhenTheLossDateIsNotSet()
    {
        var claim = TestClaims.Empty();
        claim.LossEvent.Details.LossDate.Should().Be(LossDate.Empty);

        var result = new IsLossDateNotInFuture(TestClaims.Later).Check(claim.LossEvent.Details);

        result.Findings.Should().BeEmpty();
    }
}
