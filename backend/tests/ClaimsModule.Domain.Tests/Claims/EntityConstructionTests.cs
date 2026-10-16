using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.Reserves;

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
    public void ClaimParty_CanBeBuiltWithoutArguments()
    {
        var party = (ClaimParty)Activator.CreateInstance(typeof(ClaimParty), nonPublic: true)!;

        party.Details.Should().NotBeNull();
    }

    [Fact]
    public void ClaimRiskObject_CanBeBuiltWithoutArguments()
    {
        var riskObject = (ClaimRiskObject)Activator.CreateInstance(typeof(ClaimRiskObject), nonPublic: true)!;

        riskObject.Details.Should().NotBeNull();
    }

    [Fact]
    public void ClaimLossEvent_CanBeBuiltWithoutArgumentsWithAnEmptyLoss()
    {
        var loss = (ClaimLossEvent)Activator.CreateInstance(typeof(ClaimLossEvent), nonPublic: true)!;

        loss.Details.Should().Be(LossDetails.Empty);
    }

    [Fact]
    public void ClaimValidationIssue_CanBeBuiltWithoutArgumentsWithAnEmptyFinding()
    {
        var issue = (ClaimValidationIssue)Activator.CreateInstance(typeof(ClaimValidationIssue), nonPublic: true)!;

        issue.Finding.Should().Be(new Finding(string.Empty, string.Empty));
        issue.State.Should().BeOfType<IssueRaisedState>();
    }

    [Fact]
    public void ClaimStateChange_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var stateChange = (ClaimStateChange)Activator.CreateInstance(typeof(ClaimStateChange), nonPublic: true)!;

        stateChange.Reason.Should().Be(StateChangeReason.Empty);
        stateChange.Note.Should().BeEmpty();
    }

    [Fact]
    public void Reserve_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var reserve = (Reserve)Activator.CreateInstance(typeof(Reserve), nonPublic: true)!;

        reserve.ComponentType.Should().BeOfType<ReserveIndemnityComponentType>();
        reserve.Status.Should().Be(ReserveStatus.Active);
        reserve.Notes.Should().BeEmpty();
    }

    [Fact]
    public void ReserveChange_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var change = (ReserveChange)Activator.CreateInstance(typeof(ReserveChange), nonPublic: true)!;

        change.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
        change.PostingState.Should().BeOfType<ReservePostingPendingState>();
        change.PostingDetails.Should().Be(ReservePostingDetails.Empty);
        change.Balances.Should().Be(ReserveBalances.Empty);
        change.Review.Should().Be(ReserveReview.Empty);
        change.Details.Amount.Should().Be(0m);
    }
}
