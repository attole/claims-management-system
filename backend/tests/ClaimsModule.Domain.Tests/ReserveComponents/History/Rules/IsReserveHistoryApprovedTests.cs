using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class IsReserveHistoryApprovedTests
{
    [Fact]
    [Trait("Rule", IsReserveHistoryApproved.Code)]
    [Trait("Rule", "S:JOB-GL")]
    public void Check_ForAnApprovedChange_Passes()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var rule = new IsReserveHistoryApproved();

        var result = rule.Check(history);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReserveHistoryApproved.Code)]
    [Trait("Rule", "S:JOB-GL")]
    public void Check_ForAPendingChange_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(50_000m);
        var rule = new IsReserveHistoryApproved();

        var result = rule.Check(history);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsReserveHistoryApproved.Code);
    }
}
