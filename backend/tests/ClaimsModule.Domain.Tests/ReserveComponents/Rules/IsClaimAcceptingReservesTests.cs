using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.Rules;

namespace ClaimsModule.Domain.Tests.ReserveComponents.Rules;

public sealed class IsClaimAcceptingReservesTests
{
    [Fact]
    [Trait("Rule", IsClaimAcceptingReserves.Code)]
    public void Check_ForAnEditableClaim_Passes()
    {
        var rule = new IsClaimAcceptingReserves(new ClaimOpenState());

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsClaimAcceptingReserves.Code)]
    public void Check_ForAClosedClaim_ReturnsTheFinding()
    {
        var rule = new IsClaimAcceptingReserves(new ClaimClosedState());

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsClaimAcceptingReserves.Code);
    }
}
