using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Core.Base.Flows;

public sealed class FlowOutcomeTests
{
    [Fact]
    public void Success_CarriesTheValueAndTheFindings()
    {
        var blocking = new RuleFinding("B", "b");
        var advisory = new RuleFinding("W", "w").AsWarning();
        CheckResult<DomainFinding>[] results =
        [
            CheckResult<DomainFinding>.Found(blocking),
            CheckResult<DomainFinding>.Found(advisory),
        ];
        var checkResult = CheckResult<DomainFinding>.Combine(results);

        var outcome = FlowOutcome<string, DomainFinding>.Success("done", checkResult);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("done");
        outcome.Blocking.Should().Equal(blocking);
        outcome.Advisory.Should().Equal(advisory);
    }

    [Fact]
    public void Failure_CarriesTheFindings()
    {
        var finding = new RuleFinding("E", "e");

        var outcome = FlowOutcome<string, DomainFinding>.Failure(CheckResult<DomainFinding>.Found(finding));

        outcome.IsSuccess.Should().BeFalse();
        outcome.Blocking.Should().Equal(finding);
    }

    [Fact]
    public void Value_WhenTheOutcomeFailed_Throws()
    {
        var finding = new RuleFinding("E", "e");
        var outcome = FlowOutcome<string, DomainFinding>.Failure(CheckResult<DomainFinding>.Found(finding));

        Func<string> act = () => outcome.Value;

        act.Should().Throw<InvalidOperationException>();
    }
}
