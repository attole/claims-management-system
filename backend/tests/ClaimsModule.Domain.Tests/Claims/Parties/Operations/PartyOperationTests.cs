using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Events;
using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Operations;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Operations;

public sealed class PartyOperationTests
{
    [Fact]
    public void AddParty_AddsActivePartyTouchesClaimAndRaisesEvent()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();

        OperationOutcome<ClaimParty> outcome = new AddPartyOperation().Execute(
            claim,
            new AddPartyInput(TestClaims.Witness(), TestClaims.Actor, TestClaims.Later));

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.IsActive.Should().BeTrue();
        claim.Parties.Should().HaveCount(2);
        claim.UpdatedAt.Should().Be(TestClaims.Later);
        claim.PopEvents().Should().ContainSingle().Which.Should().BeOfType<PartyAdded>();
    }

    [Fact]
    public void RemoveParty_DeactivatesPartyAndRaisesEvent()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        claim.PopEvents();
        ClaimParty witness = claim.Parties[1];

        OperationOutcome<Success> outcome = new RemovePartyOperation().Execute(
            claim,
            new RemovePartyInput(witness.Id, TestClaims.Actor, TestClaims.Later));

        outcome.IsSuccess.Should().BeTrue();
        witness.IsActive.Should().BeFalse();
        claim.PopEvents().Should().ContainSingle().Which.Should().BeOfType<PartyRemoved>();
    }

    [Fact]
    [Trait("Rule", NotLastClaimant.Code)]
    public void RemoveParty_RefusesTheLastActiveClaimant()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        ClaimParty claimant = claim.Parties[0];

        OperationOutcome<Success> outcome = new RemovePartyOperation().Execute(
            claim,
            new RemovePartyInput(claimant.Id, TestClaims.Actor, TestClaims.Later));

        outcome.IsSuccess.Should().BeFalse();
        outcome.Errors.Should().ContainSingle().Which.Code.Should().Be(NotLastClaimant.Code);
        claimant.IsActive.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", NotLastClaimant.Code)]
    public void RemoveParty_AllowsRemovingAClaimantWhenAnotherRemains()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Claimant());

        OperationOutcome<Success> outcome = new RemovePartyOperation().Execute(
            claim,
            new RemovePartyInput(claim.Parties[0].Id, TestClaims.Actor, TestClaims.Later));

        outcome.IsSuccess.Should().BeTrue();
        claim.ActiveClaimantCount.Should().Be(1);
    }

    [Fact]
    public void RemoveParty_AlreadyRemoved_FailsAndRaisesNoSecondEvent()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        RemovePartyInput input = new(claim.Parties[1].Id, TestClaims.Actor, TestClaims.Later);
        new RemovePartyOperation().Execute(claim, input);
        claim.PopEvents();

        Action removeAgain = () => new RemovePartyOperation().Execute(claim, input);

        removeAgain.Should().Throw<InvalidOperationException>();
        claim.PopEvents().Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:BR-P-02")]
    public void AddParty_AllowsSeveralPartiesOfOneRole()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant());

        new AddPartyOperation().Execute(
            claim,
            new AddPartyInput(TestClaims.Claimant(), TestClaims.Actor, TestClaims.Later));

        claim.Parties.Count(party => party.Details.Role.Equals(new ClaimantRole())).Should().Be(2);
    }
}
