using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasPermittedRoleTests
{
    [Fact]
    [Trait("Rule", HasPermittedRole.Code)]
    public void Check_WhenTheRoleIsListed_Passes()
    {
        var claim = TestClaims.Empty();
        var rule = new HasPermittedRole(TestClaims.Supervisor, TestClaims.Handler, TestClaims.Supervisor);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasPermittedRole.Code)]
    public void Check_WhenTheRoleIsNotListed_ReturnsTheFinding()
    {
        var claim = TestClaims.Empty();
        var rule = new HasPermittedRole(TestClaims.Manager, TestClaims.Handler, TestClaims.Supervisor);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasPermittedRole.Code);
    }
}
