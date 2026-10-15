using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.Reserves.Root;

namespace ClaimsModule.Domain.Tests.Claims;

public sealed class EntityConstructionTests
{
    [Fact]
    public void Claim_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var claim = (Claim)Activator.CreateInstance(typeof(Claim), nonPublic: true)!;

        claim.ClaimNumber.Should().Be(ClaimNumber.Empty);
        claim.State.Should().BeOfType<ClaimDraftState>();
        claim.PolicyId.Should().Be(Guid.Empty);
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
        var loss = (ClaimLossEvent)Activator.CreateInstance(typeof(ClaimLossEvent), nonPublic: true)!;
        var issue = (ClaimValidationIssue)Activator.CreateInstance(typeof(ClaimValidationIssue), nonPublic: true)!;

        var stateChange = (ClaimStateChange)Activator.CreateInstance(typeof(ClaimStateChange), nonPublic: true)!;

        party.Details.Should().NotBeNull();
        riskObject.Details.Should().NotBeNull();
        loss.Details.Should().Be(LossDetails.Empty);
        issue.Finding.Should().Be(IssueFinding.Empty);
        issue.State.Should().BeOfType<IssueRaisedState>();
        stateChange.Reason.Should().Be(StateChangeReason.Empty);
        stateChange.Note.Should().BeEmpty();
    }

    [Fact]
    public void ReserveComponent_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var component = (Reserve)Activator.CreateInstance(typeof(Reserve), nonPublic: true)!;

        component.ComponentType.Should().BeOfType<ReserveIndemnityComponentType>();
        component.Status.Should().Be(ReserveStatus.Active);
        component.Notes.Should().BeEmpty();
    }

    [Fact]
    public void ReserveChange_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var reserve = (ReserveChange)Activator.CreateInstance(typeof(ReserveChange), nonPublic: true)!;

        reserve.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
        reserve.PostingState.Should().BeOfType<ReservePostingPendingState>();
        reserve.PostingDetails.Should().Be(ReservePostingDetails.Empty);
        reserve.Balances.Should().Be(ReserveBalances.Empty);
        reserve.Review.Should().Be(ReserveReview.Empty);
        reserve.Details.Amount.Should().Be(0m);
    }
}
