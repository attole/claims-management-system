using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Core.Flows;

public sealed class OutcomeTests
{
    [Fact]
    public void Success_CarriesTheValueAndTheFindings()
    {
        var critical = new Finding("B", "b");
        var warning = new Finding("W", "w").AsWarning();
        CheckResult[] results =
        [
            CheckResult.Found(critical),
            CheckResult.Found(warning),
        ];
        var result = CheckResult.Combine(results);

        var outcome = Outcome<string>.Success("done", result);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("done");
        outcome.CriticalFindings.Should().Equal(critical);
        outcome.WarningFindings.Should().Equal(warning);
    }

    [Fact]
    public void Failure_CarriesTheFindings()
    {
        var finding = new Finding("E", "e");

        var outcome = Outcome<string>.Failure(CheckResult.Found(finding));

        outcome.IsSuccess.Should().BeFalse();
        outcome.CriticalFindings.Should().Equal(finding);
    }

    [Fact]
    public void Value_WhenTheOutcomeFailed_Throws()
    {
        var finding = new Finding("E", "e");
        var outcome = Outcome<string>.Failure(CheckResult.Found(finding));

        Func<string> act = () => outcome.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void WithFindings_PutsTheStepFindingsFirstAndKeepsTheValue()
    {
        var stepWarning = new Finding("S", "s").AsWarning();
        var innerWarning = new Finding("I", "i").AsWarning();
        var inner = Outcome<int>.Success(7, CheckResult.Found(innerWarning));

        var outcome = inner.WithFindings(CheckResult.Found(stepWarning));

        outcome.Value.Should().Be(7);
        outcome.WarningFindings.Should().Equal(stepWarning, innerWarning);
    }

    [Fact]
    public void WithFindings_OnAFailure_KeepsTheFailure()
    {
        var critical = new Finding("E", "e");
        var stepWarning = new Finding("S", "s").AsWarning();
        var inner = Outcome<int>.Failure(CheckResult.Found(critical));

        var outcome = inner.WithFindings(CheckResult.Found(stepWarning));

        outcome.IsSuccess.Should().BeFalse();
        outcome.CriticalFindings.Should().Equal(critical);
        outcome.WarningFindings.Should().Equal(stepWarning);
    }

    [Fact]
    public void OnSuccess_OnASuccess_RunsTheHookWithTheValueAndReturnsTheSameOutcome()
    {
        var outcome = Outcome<string>.Success("done", CheckResult.Pass);
        var seen = string.Empty;

        var result = outcome.OnSuccess(value => seen = value);

        seen.Should().Be("done");
        result.Should().BeSameAs(outcome);
    }

    [Fact]
    public void OnSuccess_OnAFailure_SkipsTheHookWithTheValue()
    {
        var outcome = Outcome<string>.Failure(CheckResult.Found(new Finding("E", "e")));
        bool isCalled = false;

        outcome.OnSuccess(_ => isCalled = true);

        isCalled.Should().BeFalse();
    }

    [Fact]
    public void OnSuccess_OnASuccess_RunsTheHookAndReturnsTheSameOutcome()
    {
        var outcome = Outcome<string>.Success("done", CheckResult.Pass);
        bool isCalled = false;

        var result = outcome.OnSuccess(() => isCalled = true);

        isCalled.Should().BeTrue();
        result.Should().BeSameAs(outcome);
    }

    [Fact]
    public void OnSuccess_OnAFailure_SkipsTheHook()
    {
        var outcome = Outcome<string>.Failure(CheckResult.Found(new Finding("E", "e")));
        bool isCalled = false;

        outcome.OnSuccess(() => isCalled = true);

        isCalled.Should().BeFalse();
    }
}
