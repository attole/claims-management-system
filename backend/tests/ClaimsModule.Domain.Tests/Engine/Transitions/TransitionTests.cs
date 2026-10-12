using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Operations;
using ClaimsModule.Domain.Engine.Transitions;
using ClaimsModule.Domain.Tests.Support.Common;
using ClaimsModule.Domain.Tests.Support.Engine;

namespace ClaimsModule.Domain.Tests.Engine.Transitions;

public sealed class TransitionTests
{
    private static readonly DateTimeOffset _start = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Execute_FromADeclaredSource_AppliesAndMovesTheSubject()
    {
        SampleSubject subject = new() { State = new OpenSampleState() };

        OperationOutcome<Success> outcome = new CloseSampleTransition()
            .Execute(subject, new SampleInput("done"), Clock());

        outcome.IsSuccess.Should().BeTrue();
        subject.State.Should().Be(new ClosedSampleState());
        subject.Log.Should().Equal("apply done");
    }

    [Fact]
    public void Execute_FromAnUndeclaredSource_ReturnsNotPermittedAndChangesNothing()
    {
        SampleSubject subject = new() { State = new DraftSampleState() };

        OperationOutcome<Success> outcome = new CloseSampleTransition()
            .Execute(subject, new SampleInput("done"), Clock());

        TransitionNotPermitted error = outcome.Errors
            .Should().ContainSingle()
            .Which.Should().BeOfType<TransitionNotPermitted>().Subject;
        error.From.Should().Be("Draft");
        error.To.Should().Be("Closed");
        error.Code.Should().Be(TransitionNotPermitted.DefaultCode);
        subject.State.Should().Be(new DraftSampleState());
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WhenAGroupRuleFails_DoesNotMoveTheSubject()
    {
        SampleSubject subject = new() { State = new OpenSampleState() };

        OperationOutcome<Success> outcome = new CloseSampleTransition()
            .Execute(subject, new SampleInput(" "), Clock());

        outcome.Errors.Select(error => error.Code).Should().Equal(CloseSampleTransition.TextRequired);
        subject.State.Should().Be(new OpenSampleState());
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void Execute_UsesTheRulesOfTheMatchingSourceGroup()
    {
        OpenSampleTransition transition = new();
        SampleSubject fromDraft = new() { State = new DraftSampleState() };
        SampleSubject fromClosed = new() { State = new ClosedSampleState() };

        OperationOutcome<Success> allowed = transition.Execute(fromDraft, new SampleInput("x"), Clock());
        OperationOutcome<Success> denied = transition.Execute(fromClosed, new SampleInput("x"), Clock());

        allowed.IsSuccess.Should().BeTrue();
        fromDraft.State.Should().Be(new OpenSampleState());
        denied.Errors.Select(error => error.Code).Should().Equal(OpenSampleTransition.ReopenDenied);
        fromClosed.State.Should().Be(new ClosedSampleState());
    }

    [Fact]
    public void Sources_AndTarget_AreDeclaredByTheTransition()
    {
        OpenSampleTransition transition = new();

        transition.Target.Should().Be(new OpenSampleState());
        transition.Sources.Should().BeEquivalentTo(
            new SampleState[] { new DraftSampleState(), new ClosedSampleState() });
    }

    [Fact]
    public void Construction_WithADuplicateSource_Throws()
    {
        Func<DuplicateSourceSampleTransition> create = () => new DuplicateSourceSampleTransition();

        create.Should().Throw<InvalidOperationException>().WithMessage("*Open*");
    }

    private static CountingTimeProvider Clock()
    {
        return new CountingTimeProvider(_start);
    }
}
