using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class IsNotSelfApprovalTests
{
    [Fact]
    [Trait("Rule", IsNotSelfApproval.Code)]
    public void Check_WhenAnotherUserApproves_Passes()
    {
        var change = TestReserves.Submit(50_000m);
        var rule = new IsNotSelfApproval(TestReserves.Approver);

        var result = rule.Check(change);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsNotSelfApproval.Code)]
    public void Check_WhenTheSubmitterApproves_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(50_000m);
        var rule = new IsNotSelfApproval(TestReserves.Submitter);

        var result = rule.Check(change);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsNotSelfApproval.Code);
    }
}
