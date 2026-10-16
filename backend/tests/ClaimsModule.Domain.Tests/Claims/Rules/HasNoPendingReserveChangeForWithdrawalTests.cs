using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasNoPendingReserveChangeForWithdrawalTests
{
    [Fact]
    [Trait("Rule", HasNoPendingReserveChangeForWithdrawal.Code)]
    public void Check_WhenItHolds_Passes()
    {
        var rule = new HasNoPendingReserveChangeForWithdrawal(false);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoPendingReserveChangeForWithdrawal.Code)]
    public void Check_WhenItDoesNotHold_ReturnsTheFinding()
    {
        var rule = new HasNoPendingReserveChangeForWithdrawal(true);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoPendingReserveChangeForWithdrawal.Code);
    }
}
