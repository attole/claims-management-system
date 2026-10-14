using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasActiveClaimantTests
{
    [Fact]
    [Trait("Rule", HasActiveClaimant.Code)]
    public void Check_WithAnActiveClaimant_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new HasActiveClaimant().Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasActiveClaimant.Code)]
    public void Check_WithoutAClaimant_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Witness());

        var result = new HasActiveClaimant().Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasActiveClaimant.Code);
    }
}
