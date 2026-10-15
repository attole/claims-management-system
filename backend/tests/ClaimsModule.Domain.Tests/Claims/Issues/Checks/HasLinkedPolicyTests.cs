using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Checks;

public sealed class HasLinkedPolicyTests
{
    [Fact]
    [Trait("Rule", HasLinkedPolicy.Code)]
    public void Check_WhenAPolicyIsLinked_Passes()
    {
        var claim = TestClaims.Empty();
        var check = new HasLinkedPolicy();

        var result = check.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasLinkedPolicy.Code)]
    public void Check_WhenNoPolicyIsLinked_ReturnsAnAdvisoryFinding()
    {
        var claim = TestClaims.WithoutPolicy();
        var check = new HasLinkedPolicy();

        var result = check.Check(claim);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(HasLinkedPolicy.Code);
        finding.IsBlocking.Should().BeFalse();
    }
}
