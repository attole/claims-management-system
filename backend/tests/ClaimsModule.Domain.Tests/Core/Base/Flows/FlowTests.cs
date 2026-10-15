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

        var outcome = PassingSampleFlow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
        outcome.Value.Should().Be("text");
        subject.Log.Should().Equal("apply");
    }

    [Fact]
    public void ABlockingRule_StopsByDefault()
    {
        var subject = new SampleSubject();

        var outcome = BlockingSampleFlow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Failed);
        outcome.Blocking.Select(finding => finding.Code).Should().Equal(BlockingSampleFlow.Blocked);
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void ABlockingRule_CanContinueAndStillApply()
    {
        var subject = new SampleSubject();

        var outcome = ContinuingSampleFlow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
        outcome.Blocking.Select(finding => finding.Code).Should().Equal(ContinuingSampleFlow.Blocked);
        subject.Log.Should().Equal("apply");
    }

    [Fact]
    public void AWarning_NeverEndsTheRun()
    {
        var subject = new SampleSubject();

        var outcome = WarningSampleFlow.Execute(subject, _input);

        outcome.Status.Should().Be(FlowStatus.Succeeded);
        outcome.Advisory.Select(finding => finding.Code).Should().Equal(WarningSampleFlow.Warned);
        subject.Log.Should().Equal("apply");
    }

    [Fact]
    public void Execute_RunsEveryCallOnTheSameInstance()
    {
        var first = new SampleSubject();
        var second = new SampleSubject();

        PassingSampleFlow.Execute(first, _input);
        PassingSampleFlow.Execute(second, _input);

        first.Log.Should().Equal("apply");
        second.Log.Should().Equal("apply");
    }
}
