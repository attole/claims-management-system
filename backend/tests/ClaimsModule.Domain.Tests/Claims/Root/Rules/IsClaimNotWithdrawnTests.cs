using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class IsClaimNotWithdrawnTests
{
    [Fact]
    [Trait("Rule", IsClaimNotWithdrawn.Code)]
    public void Check_WhenTheClaimIsClosed_Passes()
    {
        var claim = TestClaims.InState(new ClaimClosedState());

        var result = new IsClaimNotWithdrawn().Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsClaimNotWithdrawn.Code)]
    public void Check_WhenTheClaimIsWithdrawn_ReturnsTheFinding()
    {
        var claim = TestClaims.InState(new ClaimWithdrawnState());

        var result = new IsClaimNotWithdrawn().Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsClaimNotWithdrawn.Code);
    }
}
