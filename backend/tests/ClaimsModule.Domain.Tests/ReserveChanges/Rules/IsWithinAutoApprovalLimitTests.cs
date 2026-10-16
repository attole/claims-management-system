using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class IsWithinAutoApprovalLimitTests
{
    [Fact]
    [Trait("Rule", IsWithinAutoApprovalLimit.Code)]
    public void Check_WithinTheAutoLimit_Passes()
    {
        var submit = TestReserves.SubmitInput(10_000m) with { ReserveApprovedBalance = 100_000m };
        var change = TestReserves.Submit(submit);
        var rule = new IsWithinAutoApprovalLimit();

        var result = rule.Check(change);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsWithinAutoApprovalLimit.Code)]
    public void Check_AboveTheAutoLimit_ReturnsTheFinding()
    {
        var submit = TestReserves.SubmitInput(10_001m) with { ReserveApprovedBalance = 100_000m };
        var change = TestReserves.Submit(submit);
        var rule = new IsWithinAutoApprovalLimit();

        var result = rule.Check(change);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsWithinAutoApprovalLimit.Code);
    }
}
