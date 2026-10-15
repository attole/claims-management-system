using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class IsReserveChangeApprovedTests
{
    [Fact]
    [Trait("Rule", IsReserveChangeApproved.Code)]
    [Trait("Rule", "S:JOB-GL")]
    public void Check_ForAnApprovedChange_Passes()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var rule = new IsReserveChangeApproved();

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReserveChangeApproved.Code)]
    [Trait("Rule", "S:JOB-GL")]
    public void Check_ForAPendingChange_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(50_000m);
        var rule = new IsReserveChangeApproved();

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeApproved.Code);
    }
}
