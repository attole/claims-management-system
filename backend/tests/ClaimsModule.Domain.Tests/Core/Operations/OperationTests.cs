using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Operations;

public sealed class OperationTests
{
    private static readonly SampleInput _input = new("text");

    [Fact]
    public void Execute_WhenEveryRulePasses_AppliesAndReturnsTheValue()
    {
        var subject = new SampleSubject();

        var outcome = PassingSampleOperation.Execute(subject, _input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("text");
        subject.Log.Should().Equal("apply text");
    }

    [Fact]
    public void Execute_CollectsEveryErrorAndDoesNotApply()
    {
        var subject = new SampleSubject();

        var outcome = FailingSampleOperation.Execute(subject, _input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.Blocking.Select(finding => finding.Code)
            .Should().Equal(FailingSampleOperation.FirstFailure, FailingSampleOperation.SecondFailure);
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void Execute_KeepsWarningsOnSuccess()
    {
        var outcome = WarningSampleOperation.Execute(new SampleSubject(), _input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Advisory.Select(finding => finding.Code).Should().Equal(WarningSampleOperation.Warned);
    }

    [Fact]
    public void Execute_ReportsWarningsAlongsideErrors()
    {
        var outcome = MixedSampleOperation.Execute(new SampleSubject(), _input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal(MixedSampleOperation.Failed);
        outcome.Advisory.Select(finding => finding.Code).Should().Equal(MixedSampleOperation.Warned);
    }

    [Fact]
    public void Execute_WhenTheInputMakesARuleFromTheInputFail_ReturnsItsFinding()
    {
        var subject = new SampleSubject();
        var input = new SampleInput(TextSampleOperation.BadText);

        var outcome = TextSampleOperation.Execute(subject, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal(TextSampleOperation.BadInput);
    }

    [Fact]
    public void Execute_WhenTheInputMakesARuleFromTheInputPass_Succeeds()
    {
        var outcome = TextSampleOperation.Execute(new SampleSubject(), _input);

        outcome.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Execute_PassesTheTimeOfTheInputToARuleFromTheInput()
    {
        var subject = new SampleSubject();

        TimeSampleOperation.Execute(subject, _input);

        subject.Log.Should().Equal(SampleInput.Instant.ToString("O"), "apply text");
    }
}
