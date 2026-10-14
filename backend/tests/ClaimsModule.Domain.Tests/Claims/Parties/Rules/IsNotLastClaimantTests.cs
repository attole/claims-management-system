using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Rules;

public sealed class IsNotLastClaimantTests
{
    [Fact]
    [Trait("Rule", IsNotLastClaimant.Code)]
    public void Check_FailsForTheOnlyActiveClaimant()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());

        var result = new IsNotLastClaimant(claim.Parties[0].Id).Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsNotLastClaimant.Code);
    }

    [Fact]
    [Trait("Rule", IsNotLastClaimant.Code)]
    public void Check_PassesForANonClaimantWhenOneClaimantRemains()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());

        var result = new IsNotLastClaimant(claim.Parties[1].Id).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsNotLastClaimant.Code)]
    public void Check_PassesWhenAnotherActiveClaimantRemains()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Claimant());

        var result = new IsNotLastClaimant(claim.Parties[0].Id).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsNotLastClaimant.Code)]
    public void Check_PassesForAnAlreadyRemovedClaimant()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Claimant());
        var removed = claim.Parties[0].Id;
        new RemovePartyOperation().Execute(claim, new RemovePartyInput(removed, TestClaims.Actor, TestClaims.Later));

        var result = new IsNotLastClaimant(removed).Check(claim);

        result.Findings.Should().BeEmpty();
    }
}
