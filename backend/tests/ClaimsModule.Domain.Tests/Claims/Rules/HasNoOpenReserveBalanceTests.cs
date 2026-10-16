using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasNoOpenReserveBalanceTests
{
    [Fact]
    [Trait("Rule", HasNoOpenReserveBalance.Code)]
    public void Check_WhenNoBalanceIsOpen_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoOpenReserveBalance(false);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoOpenReserveBalance.Code)]
    public void Check_WhenABalanceIsOpen_ReturnsAWarning()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoOpenReserveBalance(true);

        var result = rule.Check(claim);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(HasNoOpenReserveBalance.Code);
        finding.Severity.Should().Be(FindingSeverity.Warning);
    }
}
