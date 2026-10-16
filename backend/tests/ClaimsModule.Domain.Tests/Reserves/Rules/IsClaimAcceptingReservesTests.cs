using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Reserves.Rules;

namespace ClaimsModule.Domain.Tests.Reserves.Rules;

public sealed class IsClaimAcceptingReservesTests
{
    [Fact]
    [Trait("Rule", IsClaimAcceptingReserves.Code)]
    public void Check_WhenItHolds_Passes()
    {
        var rule = new IsClaimAcceptingReserves(true);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsClaimAcceptingReserves.Code)]
    public void Check_WhenItDoesNotHold_ReturnsTheFinding()
    {
        var rule = new IsClaimAcceptingReserves(false);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsClaimAcceptingReserves.Code);
    }
}
