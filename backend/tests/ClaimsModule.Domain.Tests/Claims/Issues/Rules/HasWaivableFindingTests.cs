using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Rules;

public sealed class HasWaivableFindingTests
{
    [Fact]
    [Trait("Rule", HasWaivableFinding.Code)]
    public void Check_WhenItHolds_Passes()
    {
        var issue = TestClaims.Empty().Issues.First(issue => issue.Finding.HasCode(IsDescriptionLongEnough.Code));

        var result = new HasWaivableFinding().Check(issue);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasWaivableFinding.Code)]
    public void Check_WhenItDoesNotHold_ReturnsTheFinding()
    {
        var issue = TestClaims.Empty().Issues.First(issue => issue.Finding.HasCode(IsLossDatePresent.Code));

        var result = new HasWaivableFinding().Check(issue);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasWaivableFinding.Code);
    }
}
