using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.Root;

namespace ClaimsModule.Domain.Tests.Claims;

public sealed class EntityConstructionTests
{
    [Fact]
    public void Claim_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        Claim claim = (Claim)Activator.CreateInstance(typeof(Claim), nonPublic: true)!;

        claim.ClaimNumber.Should().Be(ClaimNumber.Empty);
        claim.State.Should().BeOfType<DraftState>();
        claim.Severity.Should().Be(ClaimSeverity.Unspecified);
        claim.LossEvent.Details.Should().Be(LossDetails.Empty);
        claim.Parties.Should().BeEmpty();
        claim.RiskObjects.Should().BeEmpty();
    }

    [Fact]
    public void ChildEntities_CanBeBuiltWithoutArguments()
    {
        ClaimParty party = (ClaimParty)Activator.CreateInstance(typeof(ClaimParty), nonPublic: true)!;
        ClaimRiskObject riskObject =
            (ClaimRiskObject)Activator.CreateInstance(typeof(ClaimRiskObject), nonPublic: true)!;
        LossEvent loss = (LossEvent)Activator.CreateInstance(typeof(LossEvent), nonPublic: true)!;

        party.Details.Should().NotBeNull();
        riskObject.Details.Should().NotBeNull();
        loss.Details.Should().Be(LossDetails.Empty);
    }
}
