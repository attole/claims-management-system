using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Claims.StateChanges.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.StateChanges.Rules;

public sealed class HasStateChangeReasonTests
{
    [Fact]
    [Trait("Rule", HasStateChangeReason.Code)]
    public void Check_WithACodeAndAText_Passes()
    {
        var claim = TestClaims.Empty();
        var reason = new StateChangeReason("CODE", "text");

        var result = new HasStateChangeReason(reason).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasStateChangeReason.Code)]
    public void Check_WithoutAReason_ReturnsTheFinding()
    {
        var claim = TestClaims.Empty();
        var reason = StateChangeReason.Empty;

        var result = new HasStateChangeReason(reason).Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasStateChangeReason.Code);
    }
}
