using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Events;
using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Operations;

public sealed class AddPartyOperationTests
{
    [Fact]
    public void AddParty_AddsActivePartyTouchesClaimAndRaisesEvent()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new AddPartyInput(TestClaims.Witness(), TestClaims.Actor, TestClaims.Later);

        var outcome = new AddPartyOperation().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.IsActive.Should().BeTrue();
        claim.Parties.Should().HaveCount(2);
        claim.UpdatedAt.Should().Be(TestClaims.Later);
        claim.PopEvents().Should().ContainSingle().Which.Should().BeOfType<PartyAdded>();
    }

    [Fact]
    [Trait("Rule", "S:BR-P-02")]
    public void AddParty_AllowsSeveralPartiesOfOneRole()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new AddPartyInput(TestClaims.Claimant(), TestClaims.Actor, TestClaims.Later);

        new AddPartyOperation().Execute(claim, input);

        claim.Parties.Count(party => party.Details.Role.Equals(new PartyClaimantRole())).Should().Be(2);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void AddParty_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState(new ClaimClosedState());
        var input = new AddPartyInput(TestClaims.Witness(), TestClaims.Actor, TestClaims.Later);

        var outcome = new AddPartyOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
