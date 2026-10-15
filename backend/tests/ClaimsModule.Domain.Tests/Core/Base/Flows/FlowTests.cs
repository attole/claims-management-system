using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Base.Flows;

public sealed class FlowTests
{
    private static readonly SampleInput _input = new("text");

    [Fact]
    public void Execute_WhenEverythingPasses_Applies()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Stop, [SampleRules.Pass()]);

        var outcome = flow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
        outcome.Value.Should().Be("text");
        subject.Log.Should().Equal("apply");
    }

    [Fact]
    public void ABlockingRule_StopsByDefault()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Stop, [SampleRules.Fail("BLOCK")]);

        var outcome = flow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Failed);
        outcome.Blocking.Select(finding => finding.Code).Should().Equal("BLOCK");
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void ABlockingRule_CanContinueAndStillApply()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Continue, [SampleRules.Fail("BLOCK")]);

        var outcome = flow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
        outcome.Blocking.Select(finding => finding.Code).Should().Equal("BLOCK");
        subject.Log.Should().Equal("apply");
    }

    [Fact]
    public void AWarning_NeverEndsTheRun()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Stop, [SampleRules.Warn("WARN")]);

        var outcome = flow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
        outcome.Advisory.Select(finding => finding.Code).Should().Equal("WARN");
        subject.Log.Should().Equal("apply");
    }
}
