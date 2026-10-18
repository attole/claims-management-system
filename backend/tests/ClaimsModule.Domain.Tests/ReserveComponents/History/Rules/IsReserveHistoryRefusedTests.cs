using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class IsReserveHistoryRefusedTests
{
    [Fact]
    [Trait("Rule", IsReserveHistoryRefused.Code)]
    public void Check_ForARejectedChange_Passes()
    {
        var history = TestReserves.SubmitRejected();
        var rule = new IsReserveHistoryRefused();

        var result = rule.Check(history);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReserveHistoryRefused.Code)]
    public void Check_ForAnApprovedChange_ReturnsTheFinding()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var rule = new IsReserveHistoryRefused();

        var result = rule.Check(history);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsReserveHistoryRefused.Code);
    }
}
