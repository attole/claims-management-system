using ClaimsModule.Domain.Claims.Parties.Flows;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Rules;

public sealed class IsPartyActiveTests
{
    [Fact]
    [Trait("Rule", IsPartyActive.Code)]
    public void Check_ForAnActiveParty_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new IsPartyActive().Check(claim.Parties[0]);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsPartyActive.Code)]
    public void Check_ForARemovedParty_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        var witness = claim.Parties[1];
        var input = new RemovePartyInput(witness.Id, TestClaims.Period(), TestClaims.Codes());
        RemovePartyOnClaimOperation.Run(claim, input);

        var result = new IsPartyActive().Check(witness);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsPartyActive.Code);
    }
}
