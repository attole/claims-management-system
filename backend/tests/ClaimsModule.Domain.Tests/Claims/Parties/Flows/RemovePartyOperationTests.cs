using ClaimsModule.Domain.Claims.Parties.Flows;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Parties.Flows;

public sealed class RemovePartyOperationTests
{
    [Fact]
    public void Run_DeactivatesTheParty()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        var witness = claim.Parties[1];

        var outcome = RemovePartyOperation.Run(witness, NoInput.Value);

        outcome.IsSuccess.Should().BeTrue();
        witness.IsActive.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", IsPartyActive.Code)]
    public void Run_ForARemovedParty_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());
        var witness = claim.Parties[1];
        RemovePartyOperation.Run(witness, NoInput.Value);

        var outcome = RemovePartyOperation.Run(witness, NoInput.Value);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsPartyActive.Code);
    }
}
