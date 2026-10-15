using ClaimsModule.Domain.Claims.Loss.Checks;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasNoUnacknowledgedPolicyWarningTests
{
    [Fact]
    [Trait("Rule", HasNoUnacknowledgedPolicyWarning.Code)]
    public void Check_WhenThePolicyPeriodWarningIsAbsent_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoUnacknowledgedPolicyWarning();

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoUnacknowledgedPolicyWarning.Code)]
    public void Check_WhenThePolicyPeriodWarningIsUnresolved_ReturnsTheFinding()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        var rule = new HasNoUnacknowledgedPolicyWarning();

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoUnacknowledgedPolicyWarning.Code);
    }

    [Fact]
    [Trait("Rule", HasNoUnacknowledgedPolicyWarning.Code)]
    public void Check_WhenThePolicyPeriodWarningIsAcknowledged_Passes()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        TestClaims.AcknowledgeIssue(claim, IsLossDateWithinPolicyPeriod.Code);
        var rule = new HasNoUnacknowledgedPolicyWarning();

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoUnacknowledgedPolicyWarning.Code)]
    public void Check_WhenOnlyAnotherWarningIsUnresolved_Passes()
    {
        var claim = TestClaims.WithPolicyWarning();
        var rule = new HasNoUnacknowledgedPolicyWarning();

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }
}
