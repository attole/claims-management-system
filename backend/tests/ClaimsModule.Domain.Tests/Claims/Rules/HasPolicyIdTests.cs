using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasPolicyIdTests
{
    [Fact]
    [Trait("Rule", HasPolicyId.Code)]
    public void Check_WithAPolicyId_Passes()
    {
        var claim = TestClaims.Empty();
        var rule = new HasPolicyId(TestClaims.Policy);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasPolicyId.Code)]
    public void Check_WithoutAPolicyId_ReturnsTheFinding()
    {
        var claim = TestClaims.Empty();
        var rule = new HasPolicyId(Guid.Empty);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasPolicyId.Code);
    }
}
