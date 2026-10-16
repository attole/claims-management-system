using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasNoOpenReserveBalanceOnWithdrawalTests
{
    [Fact]
    [Trait("Rule", HasNoOpenReserveBalanceOnWithdrawal.Code)]
    public void Check_WhenItHolds_Passes()
    {
        var rule = new HasNoOpenReserveBalanceOnWithdrawal(false);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoOpenReserveBalanceOnWithdrawal.Code)]
    public void Check_WhenItDoesNotHold_ReturnsTheFinding()
    {
        var rule = new HasNoOpenReserveBalanceOnWithdrawal(true);

        var result = rule.Check(NoSubject.Value);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(HasNoOpenReserveBalanceOnWithdrawal.Code);
        finding.Severity.Should().Be(FindingSeverity.Warning);
    }
}
