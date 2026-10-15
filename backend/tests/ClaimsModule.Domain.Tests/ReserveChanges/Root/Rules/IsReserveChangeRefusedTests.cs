using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class IsReserveChangeRefusedTests
{
    [Fact]
    [Trait("Rule", IsReserveChangeRefused.Code)]
    public void Check_ForARejectedChange_Passes()
    {
        var reserve = TestReserves.SubmitRejected();
        var rule = new IsReserveChangeRefused();

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReserveChangeRefused.Code)]
    public void Check_ForARetractedChange_Passes()
    {
        var reserve = TestReserves.SubmitRetracted();
        var rule = new IsReserveChangeRefused();

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReserveChangeRefused.Code)]
    public void Check_ForAnApprovedChange_ReturnsTheFinding()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var rule = new IsReserveChangeRefused();

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeRefused.Code);
    }

    [Fact]
    [Trait("Rule", IsReserveChangeRefused.Code)]
    public void Check_ForAPendingChange_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(50_000m);
        var rule = new IsReserveChangeRefused();

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeRefused.Code);
    }
}
