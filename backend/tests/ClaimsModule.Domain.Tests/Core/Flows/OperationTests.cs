using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Flows;

public sealed class OperationTests
{
    private static readonly SampleInput _input = new("text");

    [Fact]
    public void Run_WhenEveryRulePasses_AppliesAndReturnsTheValue()
    {
        var subject = new SampleSubject();

        var outcome = PassingSampleOperation.Run(subject, _input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("text");
        subject.Log.Should().Equal("apply text");
    }

    [Fact]
    public void Run_CollectsEveryErrorAndDoesNotApply()
    {
        var subject = new SampleSubject();

        var outcome = FailingSampleOperation.Run(subject, _input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.CriticalFindings.Select(finding => finding.Code)
            .Should().Equal(FailingSampleOperation.FirstFailure, FailingSampleOperation.SecondFailure);
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void Run_KeepsWarningsOnSuccess()
    {
        var outcome = WarningSampleOperation.Run(new SampleSubject(), _input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.WarningFindings.Select(finding => finding.Code).Should().Equal(WarningSampleOperation.Warned);
    }

    [Fact]
    public void Run_ReportsWarningsAlongsideErrors()
    {
        var outcome = MixedSampleOperation.Run(new SampleSubject(), _input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Equal(MixedSampleOperation.Failed);
        outcome.WarningFindings.Select(finding => finding.Code).Should().Equal(MixedSampleOperation.Warned);
    }

    [Fact]
    public void Run_WhenTheInputMakesARuleFromTheInputFail_ReturnsItsFinding()
    {
        var subject = new SampleSubject();
        var input = new SampleInput(TextSampleOperation.BadText);

        var outcome = TextSampleOperation.Run(subject, input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Equal(TextSampleOperation.BadInput);
    }

    [Fact]
    public void Run_WhenTheInputMakesARuleFromTheInputPass_Succeeds()
    {
        var outcome = TextSampleOperation.Run(new SampleSubject(), _input);

        outcome.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Run_RunsEveryCallOnTheSameInstance()
    {
        var first = new SampleSubject();
        var second = new SampleSubject();

        PassingSampleOperation.Run(first, _input);
        PassingSampleOperation.Run(second, _input);

        first.Log.Should().Equal("apply text");
        second.Log.Should().Equal("apply text");
    }
}
