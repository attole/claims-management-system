using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class HasApprovalRoleTests
{
    [Fact]
    [Trait("Rule", HasApprovalRole.Code)]
    public void Check_ForASupervisor_Passes()
    {
        var reserve = TestReserves.Submit(50_000m);
        var rule = new HasApprovalRole(TestClaims.Supervisor);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovalRole.Code)]
    public void Check_ForAManager_Passes()
    {
        var reserve = TestReserves.Submit(50_000m);
        var rule = new HasApprovalRole(TestClaims.Manager);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasApprovalRole.Code)]
    public void Check_ForAHandler_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(50_000m);
        var rule = new HasApprovalRole(TestClaims.Handler);

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasApprovalRole.Code);
    }
}
