using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class HasApprovalAuthorityTests
{
    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    public void Check_WhenTheRoleCoversTheAmount_Passes()
    {
        var reserve = TestReserves.Submit(50_000m);
        var rule = new HasApprovalAuthority(TestClaims.Supervisor);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    public void Check_WhenTheRoleDoesNotCoverTheAmount_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(250_000m);
        var rule = new HasApprovalAuthority(TestClaims.Supervisor);

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasApprovalAuthority.Code);
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    public void Check_ForASupervisorAtTheirLimit_Passes()
    {
        var reserve = TestReserves.Submit(100_000m);
        var rule = new HasApprovalAuthority(TestClaims.Supervisor);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    public void Check_ForAManagerAboveTenMillion_Passes()
    {
        var reserve = TestReserves.Submit(12_000_000m);
        var rule = new HasApprovalAuthority(TestClaims.Manager);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    public void Check_ForASupervisorAboveTheirLimit_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(100_001m);
        var rule = new HasApprovalAuthority(TestClaims.Supervisor);

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasApprovalAuthority.Code);
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    public void Check_ForAHandler_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(1m);
        var rule = new HasApprovalAuthority(TestClaims.Handler);

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasApprovalAuthority.Code);
    }
}
