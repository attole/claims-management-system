using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasNoPendingReserveHistoryForWithdrawalTests
{
    [Fact]
    [Trait("Rule", HasNoPendingReserveHistoryForWithdrawal.Code)]
    public void Check_WhenItHolds_Passes()
    {
        var rule = new HasNoPendingReserveHistoryForWithdrawal(false);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoPendingReserveHistoryForWithdrawal.Code)]
    public void Check_WhenItDoesNotHold_ReturnsTheFinding()
    {
        var rule = new HasNoPendingReserveHistoryForWithdrawal(true);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoPendingReserveHistoryForWithdrawal.Code);
    }
}
