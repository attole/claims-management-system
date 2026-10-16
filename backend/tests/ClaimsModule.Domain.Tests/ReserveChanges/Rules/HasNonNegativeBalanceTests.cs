using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Reserves;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class HasNonNegativeBalanceTests
{
    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void Check_WhenTheBalanceStaysAtOrAboveZero_Passes()
    {
        var rule = new HasNonNegativeBalance(0m, new ReserveIndemnityComponentType());

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void Check_WhenTheBalanceWouldGoBelowZero_ReturnsTheFinding()
    {
        var rule = new HasNonNegativeBalance(-1m, new ReserveIndemnityComponentType());

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNonNegativeBalance.Code);
    }
}
