using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Flows;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Domain.Roles.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Flows;

public sealed class WaiveIssueOnClaimOperationTests
{
    [Fact]
    public void Run_ResolvesTheCriticalIssueWithoutClearingIt()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(issue => issue.Finding.HasCode(IsDescriptionLongEnough.Code));
        var input = CreateInput(issue.Id, TestClaims.Supervisor);

        var outcome = WaiveIssueOnClaimOperation.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        issue.State.Should().BeOfType<IssueWaivedState>();
    }

    [Fact]
    public void Run_AnUnknownIssue_Throws()
    {
        var claim = TestClaims.Empty();
        var input = CreateInput(Guid.NewGuid(), TestClaims.Supervisor);

        Action act = () => WaiveIssueOnClaimOperation.Run(claim, input);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = CreateInput(claim.Issues[0].Id, TestClaims.Supervisor);

        var outcome = WaiveIssueOnClaimOperation.Run(claim, input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Contain(IsClaimEditable.Code);
    }

    [Fact]
    [Trait("Rule", HasMinimumRole.Code)]
    public void Run_ByAHandler_ReturnsTheRoleFinding()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(issue => issue.Finding.HasCode(IsDescriptionLongEnough.Code));
        var input = CreateInput(issue.Id, TestClaims.Handler);

        var outcome = WaiveIssueOnClaimOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasMinimumRole.Code);
    }

    [Fact]
    public void Run_StampedBeforeTheClaimWasReported_Throws()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(issue => issue.Finding.HasCode(IsDescriptionLongEnough.Code));
        var earlier = TestClaims.Start.AddDays(-1);
        var input = new WaiveIssueInput(
            issue.Id,
            TestClaims.Supervisor,
            TestClaims.Actor,
            earlier,
            TestClaims.LossReference());

        Action act = () => WaiveIssueOnClaimOperation.Run(claim, input);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static WaiveIssueInput CreateInput(Guid issueId, UserRole role)
    {
        var input = new WaiveIssueInput(issueId, role, TestClaims.Actor, TestClaims.Later, TestClaims.LossReference());

        return input;
    }
}
