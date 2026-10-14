using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Common;
using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Transitions;

public sealed class TransitionTests
{
    [Fact]
    public void Execute_FromADeclaredSource_MovesTheSubject()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput("done");

        var outcome = new CloseSampleTransition().Execute(subject, input);

        outcome.IsSuccess.Should().BeTrue();
        subject.State.Should().Be(new ClosedSampleState());
    }

    [Fact]
    public void Execute_WhenTheRulesPass_AppliesWhileTheSubjectIsStillInItsSource()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput("done");

        new CloseSampleTransition().Execute(subject, input);

        subject.Log.Should().Equal("apply done from Open");
    }

    [Fact]
    public void Execute_ReturnsTheNewState()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput("done");

        var outcome = new CloseSampleTransition().Execute(subject, input);

        outcome.Value.Should().Be(new ClosedSampleState());
    }

    [Fact]
    [Trait("Rule", TransitionFinding.DefaultCode)]
    public void Execute_FromAnUndeclaredSource_ReturnsNotPermittedAndChangesNothing()
    {
        var subject = new SampleSubject { State = new DraftSampleState() };
        var input = new SampleInput("done");

        var outcome = new CloseSampleTransition().Execute(subject, input);

        var finding = outcome.Blocking
            .Should().ContainSingle()
            .Which.Should().BeOfType<TransitionFinding>().Subject;
        finding.From.Should().Be("Draft");
        finding.To.Should().Be("Closed");
        finding.Code.Should().Be(TransitionFinding.DefaultCode);
        subject.State.Should().Be(new DraftSampleState());
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", TransitionFinding.DefaultCode)]
    public void Execute_FromAnUndeclaredSource_EvaluatesNoRule()
    {
        var subject = new SampleSubject { State = new ClosedSampleState() };
        var input = new SampleInput("x");

        var outcome = new CommonRuleSampleTransition().Execute(subject, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }

    [Fact]
    public void Execute_WhenAnAllowedSourceRuleFails_DoesNotMoveTheSubject()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput(" ");

        var outcome = new CloseSampleTransition().Execute(subject, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal(CloseSampleTransition.TextRequired);
        subject.State.Should().Be(new OpenSampleState());
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void Execute_FromASourceWithoutRules_MovesTheSubject()
    {
        var subject = new SampleSubject { State = new DraftSampleState() };
        var input = new SampleInput("x");

        var outcome = new OpenSampleTransition().Execute(subject, input);

        outcome.IsSuccess.Should().BeTrue();
        subject.State.Should().Be(new OpenSampleState());
    }

    [Fact]
    public void Execute_FromASourceWithRules_AppliesOnlyThoseRules()
    {
        var subject = new SampleSubject { State = new ClosedSampleState() };
        var input = new SampleInput("x");

        var outcome = new OpenSampleTransition().Execute(subject, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal(OpenSampleTransition.ReopenDenied);
        subject.State.Should().Be(new ClosedSampleState());
    }

    [Fact]
    public void Execute_AppliesACommonRuleFromTheFirstSource()
    {
        var subject = new SampleSubject { State = new DraftSampleState() };
        var input = new SampleInput("x");

        var outcome = new CommonRuleSampleTransition().Execute(subject, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal(CommonRuleSampleTransition.CommonDenied);
    }

    [Fact]
    public void Execute_AppliesACommonRuleFromTheOtherSource()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput("x");

        var outcome = new CommonRuleSampleTransition().Execute(subject, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal(CommonRuleSampleTransition.CommonDenied);
    }

    [Fact]
    public void Execute_FromTheFirstSourceOfASharedDeclaration_AppliesTheSharedRule()
    {
        var subject = new SampleSubject { State = new DraftSampleState() };
        var input = new SampleInput("x");

        var outcome = new SharedRuleSampleTransition().Execute(subject, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal(SharedRuleSampleTransition.SharedDenied);
    }

    [Fact]
    public void Execute_FromTheOtherSourceOfASharedDeclaration_AppliesTheSharedRule()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput("x");

        var outcome = new SharedRuleSampleTransition().Execute(subject, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal(SharedRuleSampleTransition.SharedDenied);
    }

    [Fact]
    public void Sources_AndTarget_AreDeclaredByTheTransition()
    {
        var transition = new OpenSampleTransition();

        transition.Target.Should().Be(new OpenSampleState());
        transition.Sources.Should().BeEquivalentTo(
            new SampleState[] { new DraftSampleState(), new ClosedSampleState() });
    }
}
