using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasApprovedReserveForClaimUnderInvestigationTests
{
    [Fact]
    [Trait("Rule", HasApprovedReserveForClaimUnderInvestigation.Code)]
    public void Check_WhenAnApprovedReserveExists_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasApprovedReserveForClaimUnderInvestigation(TestClaims.Reserves());

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovedReserveForClaimUnderInvestigation.Code)]
    public void Check_WhenNoApprovedReserveExists_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var reserves = TestClaims.Reserves() with { ApprovedReserveCount = 0 };
        var rule = new HasApprovedReserveForClaimUnderInvestigation(reserves);

        var result = rule.Check(claim);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(HasApprovedReserveForClaimUnderInvestigation.Code);
    }
}
