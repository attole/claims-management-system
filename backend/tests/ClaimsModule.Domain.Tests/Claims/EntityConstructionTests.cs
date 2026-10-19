using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;

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
    public void LossEvent_CanBeBuiltWithoutArgumentsWithAnEmptyLoss()
    {
        var loss = (LossEvent)Activator.CreateInstance(typeof(LossEvent), nonPublic: true)!;

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
    public void ReserveComponent_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var component = (ReserveComponent)Activator.CreateInstance(typeof(ReserveComponent), nonPublic: true)!;

        component.ComponentType.Should().BeOfType<ReserveIndemnityComponentType>();
        component.State.Should().BeOfType<ReserveComponentActiveState>();
        component.Notes.Should().BeEmpty();
    }

    [Fact]
    public void ClaimDocument_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var document = (ClaimDocument)Activator.CreateInstance(typeof(ClaimDocument), nonPublic: true)!;

        document.Details.DocumentName.Should().BeEmpty();
        document.Details.FileSizeBytes.Should().Be(0);
        document.Details.Notes.Should().BeEmpty();
        document.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Policy_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var policy = (Policy)Activator.CreateInstance(typeof(Policy), nonPublic: true)!;

        policy.PolicyNumber.Should().BeEmpty();
        policy.ClientName.Should().BeEmpty();
        policy.Period.Should().Be(PolicyPeriod.Empty);
        policy.CoverageTypes.Should().BeEmpty();
    }

    [Fact]
    public void CauseOfLossCode_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var code = (CauseOfLossCode)Activator.CreateInstance(typeof(CauseOfLossCode), nonPublic: true)!;

        code.Code.Should().BeEmpty();
        code.Name.Should().BeEmpty();
        code.PerilCategory.Should().Be(PerilCategory.General);
        code.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ReserveHistory_CanBeBuiltWithoutArgumentsWithEmptyValues()
    {
        var history = (ReserveHistory)Activator.CreateInstance(typeof(ReserveHistory), nonPublic: true)!;

        history.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
        history.PostingState.Should().BeOfType<ReservePostingPendingState>();
        history.PostingDetails.Should().Be(ReservePostingDetails.Empty);
        history.Balances.Should().Be(ReserveBalances.Empty);
        history.Review.Should().Be(ReserveReview.Empty);
        history.Details.Amount.Should().Be(0m);
    }
}
