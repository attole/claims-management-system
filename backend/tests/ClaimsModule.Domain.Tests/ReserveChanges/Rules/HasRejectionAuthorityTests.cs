using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class HasRejectionAuthorityTests
{
    [Fact]
    [Trait("Rule", HasRejectionAuthority.Code)]
    public void Check_WhenTheRoleCoversTheAmount_Passes()
    {
        var change = TestReserves.Submit(50_000m);
        var rule = new HasRejectionAuthority(TestClaims.Supervisor);

        var result = rule.Check(change);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasRejectionAuthority.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    [Trait("Rule", "S:ROLE-Enforcement")]
    public void Check_WhenTheRoleDoesNotCoverTheAmount_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(250_000m);
        var rule = new HasRejectionAuthority(TestClaims.Supervisor);

        var result = rule.Check(change);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasRejectionAuthority.Code);
    }
}
