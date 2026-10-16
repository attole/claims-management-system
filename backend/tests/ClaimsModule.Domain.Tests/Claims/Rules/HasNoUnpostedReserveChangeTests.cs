using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasNoUnpostedReserveChangeTests
{
    [Fact]
    [Trait("Rule", HasNoUnpostedReserveChange.Code)]
    public void Check_WhenItHolds_Passes()
    {
        var rule = new HasNoUnpostedReserveChange(false);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoUnpostedReserveChange.Code)]
    public void Check_WhenItDoesNotHold_ReturnsTheFinding()
    {
        var rule = new HasNoUnpostedReserveChange(true);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoUnpostedReserveChange.Code);
    }
}
