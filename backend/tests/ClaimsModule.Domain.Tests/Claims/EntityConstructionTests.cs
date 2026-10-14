using ClaimsModule.Domain.Claims.Issues;
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
        var claim = (Claim)Activator.CreateInstance(typeof(Claim), nonPublic: true)!;

        claim.ClaimNumber.Should().Be(ClaimNumber.Empty);
        claim.State.Should().BeOfType<ClaimDraftState>();
        claim.Severity.Should().Be(ClaimSeverity.Unspecified);
        claim.LossEvent.Details.Should().Be(LossDetails.Empty);
        claim.Parties.Should().BeEmpty();
        claim.RiskObjects.Should().BeEmpty();
        claim.Issues.Should().BeEmpty();
    }

    [Fact]
    public void ChildEntities_CanBeBuiltWithoutArguments()
    {
        var party = (ClaimParty)Activator.CreateInstance(typeof(ClaimParty), nonPublic: true)!;
        var riskObject = (ClaimRiskObject)Activator.CreateInstance(typeof(ClaimRiskObject), nonPublic: true)!;
        var loss = (LossEvent)Activator.CreateInstance(typeof(LossEvent), nonPublic: true)!;
        var issue = (ClaimValidationIssue)Activator.CreateInstance(typeof(ClaimValidationIssue), nonPublic: true)!;

        party.Details.Should().NotBeNull();
        riskObject.Details.Should().NotBeNull();
        loss.Details.Should().Be(LossDetails.Empty);
        issue.Finding.Should().Be(IssueFinding.Empty);
        issue.State.Should().BeOfType<IssueRaisedState>();
    }
}
