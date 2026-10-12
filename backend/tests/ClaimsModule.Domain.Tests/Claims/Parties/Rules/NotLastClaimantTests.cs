using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Engine.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Rules;

public sealed class NotLastClaimantTests
{
    [Fact]
    [Trait("Rule", NotLastClaimant.Code)]
    public void Check_FailsForTheOnlyActiveClaimant()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());

        RuleResult result = new NotLastClaimant(claim.Parties[0].Id).Check(claim);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be(NotLastClaimant.Code);
    }

    [Fact]
    [Trait("Rule", NotLastClaimant.Code)]
    public void Check_PassesForANonClaimantWhenOneClaimantRemains()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());

        RuleResult result = new NotLastClaimant(claim.Parties[1].Id).Check(claim);

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", NotLastClaimant.Code)]
    public void Check_PassesWhenAnotherActiveClaimantRemains()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Claimant());

        RuleResult result = new NotLastClaimant(claim.Parties[0].Id).Check(claim);

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", NotLastClaimant.Code)]
    public void Check_PassesForAnAlreadyRemovedClaimant()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Claimant());
        Guid removed = claim.Parties[0].Id;
        new RemovePartyOperation().Execute(claim, new RemovePartyInput(removed, TestClaims.Actor, TestClaims.Later));

        RuleResult result = new NotLastClaimant(removed).Check(claim);

        result.Errors.Should().BeEmpty();
    }
}
