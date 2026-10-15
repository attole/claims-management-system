using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasNoOpenReserveBalanceTests
{
    [Fact]
    [Trait("Rule", HasNoOpenReserveBalance.Code)]
    public void Check_WhenNoBalanceIsOpen_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoOpenReserveBalance(TestClaims.Reserves());

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoOpenReserveBalance.Code)]
    public void Check_WhenABalanceIsOpen_ReturnsAnAdvisoryFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var reserves = TestClaims.Reserves() with { HasOpenReserveBalance = true };
        var rule = new HasNoOpenReserveBalance(reserves);

        var result = rule.Check(claim);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(HasNoOpenReserveBalance.Code);
        finding.IsBlocking.Should().BeFalse();
    }
}
