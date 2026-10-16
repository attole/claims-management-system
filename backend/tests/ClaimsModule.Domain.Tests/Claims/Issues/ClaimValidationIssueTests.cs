using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Events;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues;

public sealed class ClaimValidationIssueTests
{
    [Fact]
    public void Clear_RaisesTheClearedEventAndEndsTheIssue()
    {
        var issue = TestClaims.Empty().Issues[0];

        issue.Clear();

        issue.State.IsCurrent.Should().BeFalse();
        issue.PopEvents().OfType<ValidationIssueClearedEvent>().Should().ContainSingle()
            .Which.IssueId.Should().Be(issue.Id);
    }

    [Fact]
    public void Waive_RaisesTheWaivedEventAndKeepsTheIssueCurrent()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(issue => issue.Finding.HasCode(IsDescriptionLongEnough.Code));
        var input = new IssueResolutionInput(TestClaims.Actor, TestClaims.Later);

        issue.Waive(input);

        issue.State.IsCurrent.Should().BeTrue();
        issue.IsUnresolvedCritical.Should().BeFalse();
        issue.PopEvents().OfType<ValidationIssueWaivedEvent>().Should().ContainSingle()
            .Which.IssueId.Should().Be(issue.Id);
    }

    [Fact]
    public void Acknowledge_ForAWarning_RaisesTheAcknowledgedEvent()
    {
        var claim = TestClaims.WithoutPolicy();
        var warning = claim.Issues.First(issue => issue.Finding.Severity == FindingSeverity.Warning);
        var input = new IssueResolutionInput(TestClaims.Actor, TestClaims.Later);

        warning.Acknowledge(input);

        warning.State.Should().BeOfType<IssueAcknowledgedState>();
        warning.IsUnresolvedWarning.Should().BeFalse();
        warning.PopEvents().OfType<ValidationIssueAcknowledgedEvent>().Should().ContainSingle()
            .Which.IssueId.Should().Be(warning.Id);
    }

    [Theory]
    [InlineData(IsLossDatePresent.Code, false)]
    [InlineData(IsCauseCodeActive.Code, false)]
    [InlineData(HasClaimant.Code, false)]
    [InlineData(IsDescriptionLongEnough.Code, true)]
    public void HasWaivableFinding_IsFalseOnlyForTheFixOnlyIssues(string code, bool isExpected)
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(issue => issue.Finding.HasCode(code));

        issue.HasWaivableFinding.Should().Be(isExpected);
    }
}
