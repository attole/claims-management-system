using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasNoUnpostedReserveHistoryTests
{
    [Fact]
    [Trait("Rule", HasNoUnpostedReserveHistory.Code)]
    public void Check_WhenItHolds_Passes()
    {
        var rule = new HasNoUnpostedReserveHistory(false);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoUnpostedReserveHistory.Code)]
    public void Check_WhenItDoesNotHold_ReturnsTheFinding()
    {
        var rule = new HasNoUnpostedReserveHistory(true);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoUnpostedReserveHistory.Code);
    }
}
