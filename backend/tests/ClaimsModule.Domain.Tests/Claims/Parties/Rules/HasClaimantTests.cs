using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Rules;

public sealed class HasClaimantTests
{
    [Fact]
    [Trait("Rule", HasClaimant.Code)]
    [Trait("Rule", "S:BR-P-01")]
    public void Check_FailsWithoutAnyParty()
    {
        var claim = TestClaims.Empty();

        var result = new HasClaimant().Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasClaimant.Code);
    }

    [Fact]
    [Trait("Rule", HasClaimant.Code)]
    public void Check_PassesWithAnActiveClaimant()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new HasClaimant().Check(claim);

        result.Findings.Should().BeEmpty();
    }
}
