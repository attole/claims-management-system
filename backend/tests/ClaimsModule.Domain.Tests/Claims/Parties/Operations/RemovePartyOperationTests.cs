using ClaimsModule.Domain.Claims.Parties.Events;
using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Operations;

public sealed class RemovePartyOperationTests
{
    [Fact]
    public void RemoveParty_DeactivatesPartyAndRaisesEvent()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        claim.PopEvents();
        var witness = claim.Parties[1];
        var input = new RemovePartyInput(witness.Id, TestClaims.Actor, TestClaims.Later);

        var outcome = new RemovePartyOperation().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        witness.IsActive.Should().BeFalse();
        claim.PopEvents().Should().ContainSingle().Which.Should().BeOfType<PartyRemovedEvent>();
    }

    [Fact]
    [Trait("Rule", IsNotLastClaimant.Code)]
    public void RemoveParty_RefusesTheLastActiveClaimant()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        var claimant = claim.Parties[0];
        var input = new RemovePartyInput(claimant.Id, TestClaims.Actor, TestClaims.Later);

        var outcome = new RemovePartyOperation().Execute(claim, input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsNotLastClaimant.Code);
        claimant.IsActive.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", IsNotLastClaimant.Code)]
    public void RemoveParty_AllowsRemovingAClaimantWhenAnotherRemains()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Claimant());
        var input = new RemovePartyInput(claim.Parties[0].Id, TestClaims.Actor, TestClaims.Later);

        var outcome = new RemovePartyOperation().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.ActiveClaimantCount.Should().Be(1);
    }

    [Fact]
    [Trait("Rule", IsPartyActive.Code)]
    public void RemoveParty_AlreadyRemoved_FailsAndRaisesNoSecondEvent()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        var input = new RemovePartyInput(claim.Parties[1].Id, TestClaims.Actor, TestClaims.Later);
        new RemovePartyOperation().Execute(claim, input);
        claim.PopEvents();

        var outcome = new RemovePartyOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsPartyActive.Code);
        claim.PopEvents().Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsPartyActive.Code)]
    public void RemoveParty_UnknownParty_Fails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new RemovePartyInput(Guid.NewGuid(), TestClaims.Actor, TestClaims.Later);

        var outcome = new RemovePartyOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsPartyActive.Code);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void RemoveParty_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState(new ClaimClosedState());
        var input = new RemovePartyInput(claim.Parties[0].Id, TestClaims.Actor, TestClaims.Later);

        var outcome = new RemovePartyOperation().Execute(claim, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Contain(IsClaimEditable.Code);
    }
}
