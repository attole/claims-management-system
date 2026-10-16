using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasNoPendingReserveChangeTests
{
    [Fact]
    [Trait("Rule", HasNoPendingReserveChange.Code)]
    public void Check_WhenNoReserveChangeIsPending_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoPendingReserveChange(false);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoPendingReserveChange.Code)]
    public void Check_WhenAReserveChangeIsPending_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoPendingReserveChange(true);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoPendingReserveChange.Code);
    }
}
