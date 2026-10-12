using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties;

public sealed class ClaimPartyTests
{
    [Fact]
    public void AnActiveClaimantParty_IsAnActiveClaimant()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());

        claim.Parties.Select(party => party.IsActiveClaimant).Should().Equal(true, false);
    }

    [Fact]
    public void ARemovedClaimantParty_IsNoLongerAnActiveClaimant()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Claimant());
        new RemovePartyOperation().Execute(
            claim,
            new RemovePartyInput(claim.Parties[0].Id, TestClaims.Actor, TestClaims.Later));

        claim.Parties[0].IsActiveClaimant.Should().BeFalse();
        claim.ActiveClaimantCount.Should().Be(1);
    }
}
