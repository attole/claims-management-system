using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Rules;

public sealed class IsPartyActiveTests
{
    [Fact]
    [Trait("Rule", IsPartyActive.Code)]
    public void Check_PassesForAnActiveParty()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new IsPartyActive(claim.Parties[0].Id).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsPartyActive.Code)]
    public void Check_FailsForAnUnknownParty()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new IsPartyActive(Guid.NewGuid()).Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsPartyActive.Code);
    }

    [Fact]
    [Trait("Rule", IsPartyActive.Code)]
    public void Check_FailsForARemovedParty()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        var partyId = claim.Parties[1].Id;
        var input = new RemovePartyInput(partyId, TestClaims.Actor, TestClaims.Later);
        new RemovePartyOperation().Execute(claim, input);

        var result = new IsPartyActive(partyId).Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsPartyActive.Code);
    }
}
