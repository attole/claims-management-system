using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasClaimantForClosureTests
{
    [Fact]
    [Trait("Rule", HasClaimantForClosure.Code)]
    public void Check_WhenAClaimantExists_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasClaimantForClosure();

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasClaimantForClosure.Code)]
    public void Check_WhenNoClaimantExists_ReturnsTheFinding()
    {
        var claim = TestClaims.Create();
        var rule = new HasClaimantForClosure();

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasClaimantForClosure.Code);
    }
}
