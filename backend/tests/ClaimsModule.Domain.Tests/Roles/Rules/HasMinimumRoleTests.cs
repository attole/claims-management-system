using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Roles.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Roles.Rules;

public sealed class HasMinimumRoleTests
{
    [Fact]
    [Trait("Rule", HasMinimumRole.Code)]
    public void Check_WithTheRightsOfTheMinimumRole_Passes()
    {
        var rule = new HasMinimumRole(TestClaims.Manager, TestClaims.Supervisor);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasMinimumRole.Code)]
    public void Check_WithoutTheRightsOfTheMinimumRole_ReturnsTheFinding()
    {
        var rule = new HasMinimumRole(TestClaims.Handler, TestClaims.Supervisor);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasMinimumRole.Code);
    }
}
