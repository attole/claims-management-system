using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Base.Flows;

public sealed class FlowTests
{
    private static readonly SampleInput _input = new("text");

    private static readonly IReadOnlyList<DomainFinding> _earlyFindings = [new RuleFinding("EARLY", "early")];

    [Fact]
    public void Execute_WhenEverythingPasses_AppliesAndRunsTheFollowUp()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Stop, FlowDecision.Stop, _ => [], [SampleRules.Pass()]);

        var outcome = flow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
        outcome.Value.Should().Be("text");
        subject.Log.Should().Equal("apply", "after");
    }

    [Fact]
    public void AFailedPrecondition_StopsByDefault()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Stop, FlowDecision.Stop, _ => _earlyFindings, [SampleRules.Pass()]);

        var outcome = flow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Failed);
        outcome.Blocking.Select(finding => finding.Code).Should().Equal("EARLY");
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void AFailedPrecondition_CanSkipTheFlowWithoutAnError()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Skip, FlowDecision.Stop, _ => _earlyFindings, [SampleRules.Pass()]);

        var outcome = flow.Execute(subject, _input);

        outcome.IsSkipped.Should().BeTrue();
        outcome.IsSuccess.Should().BeTrue();
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void Value_WhenTheFlowWasSkipped_Throws()
    {
        var flow = new SampleFlow(FlowDecision.Skip, FlowDecision.Stop, _ => _earlyFindings, [SampleRules.Pass()]);
        var outcome = flow.Execute(new SampleSubject(), _input);

        Func<string> act = () => outcome.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AFailedPrecondition_CanContinueAndStillReportTheFinding()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Continue, FlowDecision.Stop, _ => _earlyFindings, [SampleRules.Pass()]);

        var outcome = flow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
        outcome.Blocking.Select(finding => finding.Code).Should().Equal("EARLY");
        subject.Log.Should().Equal("apply", "after");
    }

    [Fact]
    public void ABlockingRule_StopsByDefault()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Stop, FlowDecision.Stop, _ => [], [SampleRules.Fail("BLOCK")]);

        var outcome = flow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Failed);
        outcome.Blocking.Select(finding => finding.Code).Should().Equal("BLOCK");
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void ABlockingRule_CanSkipTheFlowWithoutAnError()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Stop, FlowDecision.Skip, _ => [], [SampleRules.Fail("BLOCK")]);

        var outcome = flow.Execute(subject, _input);

        outcome.IsSkipped.Should().BeTrue();
        outcome.Blocking.Select(finding => finding.Code).Should().Equal("BLOCK");
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void ABlockingRule_CanContinueAndStillApply()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Stop, FlowDecision.Continue, _ => [], [SampleRules.Fail("BLOCK")]);

        var outcome = flow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
        outcome.Blocking.Select(finding => finding.Code).Should().Equal("BLOCK");
        subject.Log.Should().Equal("apply", "after");
    }

    [Fact]
    public void AWarning_NeverEndsTheRun()
    {
        var subject = new SampleSubject();
        var flow = new SampleFlow(FlowDecision.Stop, FlowDecision.Stop, _ => [], [SampleRules.Warn("WARN")]);

        var outcome = flow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
        outcome.Advisory.Select(finding => finding.Code).Should().Equal("WARN");
        subject.Log.Should().Equal("apply", "after");
    }

    [Fact]
    public void APrecondition_WhenTheInputMakesItFail_FailsTheRun()
    {
        var flow = CreateFlowFailingEarlyOnBadText();
        var subject = new SampleSubject();
        var input = new SampleInput("bad");

        var outcome = flow.Execute(subject, input);

        outcome.Status.Should().Be(FlowStatus.Failed);
    }

    [Fact]
    public void APrecondition_WhenTheInputMakesItPass_SucceedsTheRun()
    {
        var flow = CreateFlowFailingEarlyOnBadText();
        var subject = new SampleSubject();
        var input = new SampleInput("good");

        var outcome = flow.Execute(subject, input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
    }

    private static SampleFlow CreateFlowFailingEarlyOnBadText()
    {
        Func<SampleInput, IReadOnlyList<DomainFinding>> early =
            input => input.Text == "bad" ? _earlyFindings : [];

        return new SampleFlow(FlowDecision.Stop, FlowDecision.Stop, early, [SampleRules.Pass()]);
    }
}
