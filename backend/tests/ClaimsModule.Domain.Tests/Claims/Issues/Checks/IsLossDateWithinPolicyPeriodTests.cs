using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Checks;

public sealed class IsLossDateWithinPolicyPeriodTests
{
    [Fact]
    [Trait("Rule", IsLossDateWithinPolicyPeriod.Code)]
    public void Check_WhenTheLossIsInsideThePeriod_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var check = new IsLossDateWithinPolicyPeriod(TestClaims.Reference().PolicyPeriod);

        var result = check.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossDateWithinPolicyPeriod.Code)]
    public void Check_WhenTheLossIsBeforeThePeriod_ReturnsAnAdvisoryFinding()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        var check = new IsLossDateWithinPolicyPeriod(TestClaims.Reference().PolicyPeriod);

        var result = check.Check(claim);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsLossDateWithinPolicyPeriod.Code);
        finding.IsBlocking.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", IsLossDateWithinPolicyPeriod.Code)]
    public void Check_WhenThePolicyWasCancelledBeforeTheLoss_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var period = new PolicyPeriod(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), new DateOnly(2026, 6, 30));
        var check = new IsLossDateWithinPolicyPeriod(period);

        var result = check.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsLossDateWithinPolicyPeriod.Code);
    }

    [Fact]
    [Trait("Rule", IsLossDateWithinPolicyPeriod.Code)]
    public void Check_UsesTheUtcDayOfALossGivenInAnotherOffset_ReturnsTheFinding()
    {
        var lossDate = new DateTimeOffset(2026, 1, 1, 0, 30, 0, TimeSpan.FromHours(2));
        var claim = TestClaims.WithLoss(lossDate);
        var check = new IsLossDateWithinPolicyPeriod(TestClaims.Reference().PolicyPeriod);

        var result = check.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsLossDateWithinPolicyPeriod.Code);
    }

    [Fact]
    [Trait("Rule", IsLossDateWithinPolicyPeriod.Code)]
    public void Check_WhenThePeriodIsNotKnown_Passes()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        var check = new IsLossDateWithinPolicyPeriod(PolicyPeriod.Empty);

        var result = check.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossDateWithinPolicyPeriod.Code)]
    public void Check_WhenNoLossDateIsGiven_Passes()
    {
        var claim = TestClaims.Empty();
        var check = new IsLossDateWithinPolicyPeriod(TestClaims.Reference().PolicyPeriod);

        var result = check.Check(claim);

        result.Findings.Should().BeEmpty();
    }
}
