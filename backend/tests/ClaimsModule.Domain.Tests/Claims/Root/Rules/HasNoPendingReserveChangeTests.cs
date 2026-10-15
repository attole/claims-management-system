using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasNoPendingReserveChangeTests
{
    [Fact]
    [Trait("Rule", HasNoPendingReserveChange.Code)]
    public void Check_WhenNoReserveChangeIsPending_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoPendingReserveChange(TestClaims.Reserves());

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoPendingReserveChange.Code)]
    public void Check_WhenAReserveChangeIsPending_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var reserves = TestClaims.Reserves() with { HasPendingReserveChange = true };
        var rule = new HasNoPendingReserveChange(reserves);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoPendingReserveChange.Code);
    }
}
