using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

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
    public void Check_WhenNoPolicyIsLinked_ReturnsAWarning()
    {
        var claim = TestClaims.WithoutPolicy();
        var check = new HasLinkedPolicy();

        var result = check.Check(claim);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(HasLinkedPolicy.Code);
        finding.Severity.Should().Be(FindingSeverity.Warning);
    }
}
