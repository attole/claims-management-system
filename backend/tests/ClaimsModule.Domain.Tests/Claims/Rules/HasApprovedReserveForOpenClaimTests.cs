using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasApprovedReserveForOpenClaimTests
{
    [Fact]
    [Trait("Rule", HasApprovedReserveForOpenClaim.Code)]
    public void Check_WhenAnApprovedReserveExists_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasApprovedReserveForOpenClaim(true);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovedReserveForOpenClaim.Code)]
    public void Check_WhenNoApprovedReserveExists_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasApprovedReserveForOpenClaim(false);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasApprovedReserveForOpenClaim.Code);
    }
}
