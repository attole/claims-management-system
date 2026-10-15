using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Checks;

public sealed class HasRiskObjectTests
{
    [Fact]
    [Trait("Rule", HasRiskObject.Code)]
    public void Check_WhenARiskObjectIsLinked_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new HasRiskObject().Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasRiskObject.Code)]
    public void Check_WhenNoRiskObjectIsLinked_ReturnsAnAdvisoryFinding()
    {
        var claim = TestClaims.Empty();

        var result = new HasRiskObject().Check(claim);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(HasRiskObject.Code);
        finding.IsBlocking.Should().BeFalse();
    }
}
