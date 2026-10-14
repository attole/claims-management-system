using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasNoBlockingIssuesTests
{
    [Fact]
    [Trait("Rule", HasNoBlockingIssues.Code)]
    public void Check_WhenTheClaimIsComplete_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new HasNoBlockingIssues().Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoBlockingIssues.Code)]
    public void Check_WhenACriticalIssueIsUnresolved_ReturnsTheFinding()
    {
        var claim = TestClaims.Empty();

        var result = new HasNoBlockingIssues().Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoBlockingIssues.Code);
    }

    [Fact]
    [Trait("Rule", HasNoBlockingIssues.Code)]
    public void Check_WhenTheOnlyCriticalIssueIsWaived_Passes()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start.AddDays(2));
        TestClaims.WaiveIssue(claim, IsLossDateNotInFuture.Code);

        var result = new HasNoBlockingIssues().Check(claim);

        result.Findings.Should().BeEmpty();
    }
}
