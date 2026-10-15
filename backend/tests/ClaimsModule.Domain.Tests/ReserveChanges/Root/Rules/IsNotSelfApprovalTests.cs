using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class IsNotSelfApprovalTests
{
    [Fact]
    [Trait("Rule", IsNotSelfApproval.Code)]
    public void Check_WhenAnotherUserApproves_Passes()
    {
        var reserve = TestReserves.Submit(50_000m);
        var rule = new IsNotSelfApproval(TestReserves.Approver);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsNotSelfApproval.Code)]
    public void Check_WhenTheSubmitterApproves_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(50_000m);
        var rule = new IsNotSelfApproval(TestReserves.Submitter);

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsNotSelfApproval.Code);
    }
}
