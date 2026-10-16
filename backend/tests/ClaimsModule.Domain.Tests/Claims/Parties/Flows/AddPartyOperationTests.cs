using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Flows;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Flows;

public sealed class AddPartyOperationTests
{
    [Fact]
    public void Run_AddsAnActiveParty()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new AddPartyInput(TestClaims.Witness(), TestClaims.LossReference());

        var outcome = AddPartyOperation.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.IsActive.Should().BeTrue();
        claim.Parties.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Rule", "S:BR-P-02")]
    public void Run_AllowsSeveralPartiesOfOneRole()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new AddPartyInput(TestClaims.Claimant(), TestClaims.LossReference());

        AddPartyOperation.Run(claim, input);

        claim.Parties.Count(party => party.Details.Role.Equals(new PartyClaimantRole())).Should().Be(2);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new AddPartyInput(TestClaims.Witness(), TestClaims.LossReference());

        var outcome = AddPartyOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
