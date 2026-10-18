using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class IsWithinAutoApprovalLimitTests
{
    [Fact]
    [Trait("Rule", IsWithinAutoApprovalLimit.Code)]
    public void Check_WithinTheAutoLimit_Passes()
    {
        var history = TestReserves.Submit(10_000m);
        var rule = new IsWithinAutoApprovalLimit();

        var result = rule.Check(history);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsWithinAutoApprovalLimit.Code)]
    public void Check_AboveTheAutoLimit_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(10_001m);
        var rule = new IsWithinAutoApprovalLimit();

        var result = rule.Check(history);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsWithinAutoApprovalLimit.Code);
    }
}
