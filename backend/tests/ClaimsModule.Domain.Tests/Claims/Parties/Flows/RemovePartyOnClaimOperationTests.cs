using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Parties.Flows;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Flows;

public sealed class RemovePartyOnClaimOperationTests
{
    [Fact]
    public void Run_DeactivatesTheParty()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        claim.PopEvents();
        var witness = claim.Parties[1];
        var input = new RemovePartyInput(witness.Id, TestClaims.LossReference());

        var outcome = RemovePartyOnClaimOperation.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        witness.IsActive.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", IsNotLastClaimant.Code)]
    public void Run_RefusesTheLastActiveClaimant()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        var claimant = claim.Parties[0];
        var input = new RemovePartyInput(claimant.Id, TestClaims.LossReference());

        var outcome = RemovePartyOnClaimOperation.Run(claim, input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsNotLastClaimant.Code);
        claimant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Run_UnknownParty_Throws()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new RemovePartyInput(Guid.NewGuid(), TestClaims.LossReference());

        Action act = () => RemovePartyOnClaimOperation.Run(claim, input);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new RemovePartyInput(claim.Parties[0].Id, TestClaims.LossReference());

        var outcome = RemovePartyOnClaimOperation.Run(claim, input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Contain(IsClaimEditable.Code);
    }
}
