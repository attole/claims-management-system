using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Common;
using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Flows;

public sealed class TransitionTests
{
    [Fact]
    public void Run_FromADeclaredSource_MovesTheSubject()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput("done");

        var outcome = CloseSampleTransition.Run(subject, input);

        outcome.IsSuccess.Should().BeTrue();
        subject.State.Should().Be(new ClosedSampleState());
    }

    [Fact]
    public void Run_WhenTheRulesPass_AppliesWhileTheSubjectIsStillInItsSource()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput("done");

        CloseSampleTransition.Run(subject, input);

        subject.Log.Should().Equal("apply done from Open");
    }

    [Fact]
    public void Run_ReturnsTheNewState()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput("done");

        var outcome = CloseSampleTransition.Run(subject, input);

        outcome.Value.To.Should().Be(new ClosedSampleState());
    }

    [Fact]
    [Trait("Rule", CloseSampleTransition.NotPermittedCode)]
    public void Run_FromAnUndeclaredSource_ReturnsNotPermittedAndChangesNothing()
    {
        var subject = new SampleSubject { State = new DraftSampleState() };
        var input = new SampleInput("done");

        var outcome = CloseSampleTransition.Run(subject, input);

        var finding = outcome.CriticalFindings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(CloseSampleTransition.NotPermittedCode);
        finding.Message.Should().Contain("Draft").And.Contain("Closed");
        finding.Severity.Should().Be(FindingSeverity.Critical);
        finding.Field.Should().BeEmpty();
        subject.State.Should().Be(new DraftSampleState());
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", CloseSampleTransition.NotPermittedCode)]
    public void Run_FromAnUndeclaredSource_EvaluatesNoRule()
    {
        var subject = new SampleSubject { State = new ClosedSampleState() };
        var input = new SampleInput("x");

        var outcome = CommonRuleSampleTransition.Run(subject, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(CloseSampleTransition.NotPermittedCode);
    }

    [Fact]
    [Trait("Rule", CloseSampleTransition.NotPermittedCode)]
    public void Run_ThroughTheOperationType_StillAnswersAnUndeclaredSource()
    {
        var subject = new SampleSubject { State = new DraftSampleState() };
        var input = new SampleInput("done");

        var outcome = Operation<CloseSampleTransition, SampleSubject, SampleInput, Move<SampleState>>
            .Run(subject, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(CloseSampleTransition.NotPermittedCode);
    }

    [Fact]
    public void Run_WhenAnAllowedSourceRuleFails_DoesNotMoveTheSubject()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput(" ");

        var outcome = CloseSampleTransition.Run(subject, input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Equal(CloseSampleTransition.TextRequired);
        subject.State.Should().Be(new OpenSampleState());
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void Run_FromASourceWithoutRules_MovesTheSubject()
    {
        var subject = new SampleSubject { State = new DraftSampleState() };
        var input = new SampleInput("x");

        var outcome = OpenSampleTransition.Run(subject, input);

        outcome.IsSuccess.Should().BeTrue();
        subject.State.Should().Be(new OpenSampleState());
    }

    [Fact]
    public void Run_FromASourceWithRules_AppliesOnlyThoseRules()
    {
        var subject = new SampleSubject { State = new ClosedSampleState() };
        var input = new SampleInput("x");

        var outcome = OpenSampleTransition.Run(subject, input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Equal(OpenSampleTransition.ReopenDenied);
        subject.State.Should().Be(new ClosedSampleState());
    }

    [Fact]
    public void Run_AppliesACommonRuleFromTheFirstSource()
    {
        var subject = new SampleSubject { State = new DraftSampleState() };
        var input = new SampleInput("x");

        var outcome = CommonRuleSampleTransition.Run(subject, input);

        outcome.CriticalFindings.Select(finding => finding.Code)
            .Should().Equal(CommonRuleSampleTransition.CommonDenied);
    }

    [Fact]
    public void Run_AppliesACommonRuleFromTheOtherSource()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput("x");

        var outcome = CommonRuleSampleTransition.Run(subject, input);

        outcome.CriticalFindings.Select(finding => finding.Code)
            .Should().Equal(CommonRuleSampleTransition.CommonDenied);
    }

    [Fact]
    public void Run_FromTheFirstSourceOfASharedDeclaration_AppliesTheSharedRule()
    {
        var subject = new SampleSubject { State = new DraftSampleState() };
        var input = new SampleInput("x");

        var outcome = SharedRuleSampleTransition.Run(subject, input);

        outcome.CriticalFindings.Select(finding => finding.Code)
            .Should().Equal(SharedRuleSampleTransition.SharedDenied);
    }

    [Fact]
    public void Run_FromTheOtherSourceOfASharedDeclaration_AppliesTheSharedRule()
    {
        var subject = new SampleSubject { State = new OpenSampleState() };
        var input = new SampleInput("x");

        var outcome = SharedRuleSampleTransition.Run(subject, input);

        outcome.CriticalFindings.Select(finding => finding.Code)
            .Should().Equal(SharedRuleSampleTransition.SharedDenied);
    }

    [Fact]
    public void AllowedFrom_WithADuplicateSource_Throws()
    {
        Func<DuplicateSourceSampleTransition> act = () => new DuplicateSourceSampleTransition();

        act.Should().Throw<InvalidOperationException>().WithMessage("*Open*");
    }

    [Fact]
    public void AllowedFrom_WithoutSources_Throws()
    {
        Func<NoSourcesSampleTransition> act = () => new NoSourcesSampleTransition();

        act.Should().Throw<ArgumentException>();
    }
}
