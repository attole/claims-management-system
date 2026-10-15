using ClaimsModule.Domain.Claims.Parties.Flows;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties;

public sealed class ClaimPartyTests
{
    [Fact]
    public void AnActiveClaimantParty_IsAnActiveClaimant()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());

        claim.Parties.Select(party => party.IsActiveClaimant).Should().Equal(true, false);
    }

    [Fact]
    public void ARemovedClaimantParty_IsNoLongerAnActiveClaimant()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Claimant());
        var input = new RemovePartyInput(
            claim.Parties[0].Id,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());
        RemovePartyOperation.Execute(claim, input);

        claim.Parties[0].IsActiveClaimant.Should().BeFalse();
        claim.ActiveClaimantCount.Should().Be(1);
    }
}
