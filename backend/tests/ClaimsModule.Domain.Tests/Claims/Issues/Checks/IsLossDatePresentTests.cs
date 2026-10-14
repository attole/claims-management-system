using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Checks;

public sealed class IsLossDatePresentTests
{
    [Fact]
    [Trait("Rule", IsLossDatePresent.Code)]
    public void Check_FailsWhenTheLossDateIsNotSet()
    {
        var claim = TestClaims.Empty();

        var result = new IsLossDatePresent().Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsLossDatePresent.Code);
    }

    [Fact]
    [Trait("Rule", IsLossDatePresent.Code)]
    public void Check_PassesWhenTheLossDateIsSet()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new IsLossDatePresent().Check(claim);

        result.Findings.Should().BeEmpty();
    }
}
