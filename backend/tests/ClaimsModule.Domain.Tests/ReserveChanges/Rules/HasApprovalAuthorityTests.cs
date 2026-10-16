using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class HasApprovalAuthorityTests
{
    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    public void Check_WhenTheRoleCoversTheAmount_Passes()
    {
        var change = TestReserves.Submit(50_000m);
        var rule = new HasApprovalAuthority(TestClaims.Supervisor);

        var result = rule.Check(change);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    [Trait("Rule", "S:ROLE-Enforcement")]
    [Trait("Rule", "S:V-ReserveApproval-Role")]
    public void Check_WhenTheRoleDoesNotCoverTheAmount_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(250_000m);
        var rule = new HasApprovalAuthority(TestClaims.Supervisor);

        var result = rule.Check(change);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasApprovalAuthority.Code);
    }
}
