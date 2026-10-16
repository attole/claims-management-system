using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasAssignedHandlerTests
{
    [Fact]
    [Trait("Rule", HasAssignedHandler.Code)]
    public void Check_WhenItHolds_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new HasAssignedHandler().Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasAssignedHandler.Code)]
    public void Check_WhenItDoesNotHold_ReturnsTheFinding()
    {
        var claim = TestClaims.Empty();

        var result = new HasAssignedHandler().Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasAssignedHandler.Code);
    }
}
