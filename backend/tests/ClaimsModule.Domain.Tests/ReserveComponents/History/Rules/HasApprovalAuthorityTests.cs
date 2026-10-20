using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class HasApprovalAuthorityTests
{
    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    public void Check_WhenTheRoleCoversTheAmount_Passes()
    {
        var history = TestReserves.Submit(50_000m);
        var rule = new HasApprovalAuthority(TestClaims.Supervisor);

        var result = rule.Check(history);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    [Trait("Rule", "S:ROLE-Enforcement")]
    [Trait("Rule", "S:V-ReserveApproval-Role")]
    public void Check_WhenTheRoleDoesNotCoverTheAmount_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(250_000m);
        var rule = new HasApprovalAuthority(TestClaims.Supervisor);

        var result = rule.Check(history);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasApprovalAuthority.Code);
    }

    [Fact]
    [Trait("Rule", "S:V-Messages")]
    public void Check_WhenTheRoleDoesNotCoverTheAmount_ReturnsTheSpecMessage()
    {
        var history = TestReserves.Submit(250_000m);
        var rule = new HasApprovalAuthority(TestClaims.Supervisor);

        var result = rule.Check(history);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Message.Should().Be("Your role does not have authority to approve this reserve amount.");
    }
}
