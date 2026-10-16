using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class IsReserveChangeRefusedTests
{
    [Fact]
    [Trait("Rule", IsReserveChangeRefused.Code)]
    public void Check_ForARejectedChange_Passes()
    {
        var change = TestReserves.SubmitRejected();
        var rule = new IsReserveChangeRefused();

        var result = rule.Check(change);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReserveChangeRefused.Code)]
    public void Check_ForAnApprovedChange_ReturnsTheFinding()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var rule = new IsReserveChangeRefused();

        var result = rule.Check(change);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsReserveChangeRefused.Code);
    }
}
