using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class IsReserveChangeApprovedTests
{
    [Fact]
    [Trait("Rule", IsReserveChangeApproved.Code)]
    [Trait("Rule", "S:JOB-GL")]
    public void Check_ForAnApprovedChange_Passes()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var rule = new IsReserveChangeApproved();

        var result = rule.Check(change);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReserveChangeApproved.Code)]
    [Trait("Rule", "S:JOB-GL")]
    public void Check_ForAPendingChange_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(50_000m);
        var rule = new IsReserveChangeApproved();

        var result = rule.Check(change);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeApproved.Code);
    }
}
