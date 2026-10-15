using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasApprovedReserveForOpenClaimTests
{
    [Fact]
    [Trait("Rule", HasApprovedReserveForOpenClaim.Code)]
    public void Check_WhenAnApprovedReserveExists_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasApprovedReserveForOpenClaim(TestClaims.Reserves());

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovedReserveForOpenClaim.Code)]
    public void Check_WhenNoApprovedReserveExists_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var reserves = TestClaims.Reserves() with { ApprovedReserveCount = 0 };
        var rule = new HasApprovedReserveForOpenClaim(reserves);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasApprovedReserveForOpenClaim.Code);
    }
}
