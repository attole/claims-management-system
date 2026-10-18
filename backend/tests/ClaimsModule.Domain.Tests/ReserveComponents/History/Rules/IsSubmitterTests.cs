using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class IsSubmitterTests
{
    [Fact]
    [Trait("Rule", IsSubmitter.Code)]
    public void Check_WhenTheSubmitterActs_Passes()
    {
        var history = TestReserves.Submit(50_000m);
        var rule = new IsSubmitter(TestReserves.Submitter);

        var result = rule.Check(history);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsSubmitter.Code)]
    public void Check_WhenAnotherUserActs_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(50_000m);
        var rule = new IsSubmitter(TestReserves.Approver);

        var result = rule.Check(history);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsSubmitter.Code);
    }
}
