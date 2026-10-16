using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasLiabilityConfirmationTests
{
    [Fact]
    [Trait("Rule", HasLiabilityConfirmation.Code)]
    public void Check_WhenLiabilityIsConfirmed_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasLiabilityConfirmation(true);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasLiabilityConfirmation.Code)]
    public void Check_WhenLiabilityIsNotConfirmed_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasLiabilityConfirmation(false);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasLiabilityConfirmation.Code);
    }
}
