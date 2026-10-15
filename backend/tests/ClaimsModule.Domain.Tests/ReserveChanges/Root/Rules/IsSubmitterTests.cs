using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class IsSubmitterTests
{
    [Fact]
    [Trait("Rule", IsSubmitter.Code)]
    public void Check_WhenTheSubmitterActs_Passes()
    {
        var reserve = TestReserves.Submit(50_000m);
        var rule = new IsSubmitter(TestReserves.Submitter);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsSubmitter.Code)]
    public void Check_WhenAnotherUserActs_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(50_000m);
        var rule = new IsSubmitter(TestReserves.Approver);

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsSubmitter.Code);
    }
}
