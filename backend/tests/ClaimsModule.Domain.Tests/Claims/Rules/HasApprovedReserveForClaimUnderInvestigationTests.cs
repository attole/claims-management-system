using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasApprovedReserveForClaimUnderInvestigationTests
{
    [Fact]
    [Trait("Rule", HasApprovedReserveForClaimUnderInvestigation.Code)]
    public void Check_WhenAnApprovedReserveExists_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasApprovedReserveForClaimUnderInvestigation(true);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovedReserveForClaimUnderInvestigation.Code)]
    public void Check_WhenNoApprovedReserveExists_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasApprovedReserveForClaimUnderInvestigation(false);

        var result = rule.Check(claim);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(HasApprovedReserveForClaimUnderInvestigation.Code);
    }
}
