using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class IsNotSelfApprovalTests
{
    [Fact]
    [Trait("Rule", IsNotSelfApproval.Code)]
    public void Check_WhenAnotherUserApproves_Passes()
    {
        var history = TestReserves.Submit(50_000m);
        var rule = new IsNotSelfApproval(TestReserves.Approver);

        var result = rule.Check(history);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsNotSelfApproval.Code)]
    public void Check_WhenTheSubmitterApproves_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(50_000m);
        var rule = new IsNotSelfApproval(TestReserves.Submitter);

        var result = rule.Check(history);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsNotSelfApproval.Code);
    }
}
