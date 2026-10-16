using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class IsLossDatePresentTests
{
    [Fact]
    [Trait("Rule", IsLossDatePresent.Code)]
    public void Check_FailsWhenTheLossDateIsNotSet()
    {
        var claim = TestClaims.Empty();

        var result = new IsLossDatePresent().Check(claim.LossEvent.Details);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsLossDatePresent.Code);
    }

    [Fact]
    [Trait("Rule", IsLossDatePresent.Code)]
    public void Check_PassesWhenTheLossDateIsSet()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new IsLossDatePresent().Check(claim.LossEvent.Details);

        result.Findings.Should().BeEmpty();
    }
}
