using ClaimsModule.Domain.Engine.Operations;
using ClaimsModule.Domain.Tests.Support.Engine;

namespace ClaimsModule.Domain.Tests.Engine.Operations;

public sealed class OperationTests
{
    private static readonly SampleInput _input = new("text");

    [Fact]
    public void Execute_WhenEveryRulePasses_AppliesAndReturnsTheValue()
    {
        SampleOperation operation = new([SampleRules.Pass(), SampleRules.Pass()], []);
        SampleSubject subject = new();

        OperationOutcome<string> outcome = operation.Execute(subject, _input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("text");
        subject.Log.Should().Equal("apply text");
    }

    [Fact]
    public void Execute_CollectsEveryErrorAndDoesNotApply()
    {
        SampleOperation operation = new([SampleRules.Fail("A"), SampleRules.Pass(), SampleRules.Fail("B")], []);
        SampleSubject subject = new();

        OperationOutcome<string> outcome = operation.Execute(subject, _input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.Errors.Select(error => error.Code).Should().Equal("A", "B");
        operation.ApplyCount.Should().Be(0);
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void Execute_KeepsWarningsOnSuccess()
    {
        SampleOperation operation = new([SampleRules.Warn("W")], []);

        OperationOutcome<string> outcome = operation.Execute(new SampleSubject(), _input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Warnings.Select(warning => warning.Code).Should().Equal("W");
    }

    [Fact]
    public void Execute_ReportsWarningsAlongsideErrors()
    {
        SampleOperation operation = new([SampleRules.Warn("W"), SampleRules.Fail("E")], []);

        OperationOutcome<string> outcome = operation.Execute(new SampleSubject(), _input);

        outcome.Errors.Select(error => error.Code).Should().Equal("E");
        outcome.Warnings.Select(warning => warning.Code).Should().Equal("W");
    }

    [Fact]
    public void Execute_BuildsARuleFromTheInput()
    {
        SampleOperation operation = new(
            [],
            [input => input.Text == "bad" ? SampleRules.Fail("BAD_INPUT") : SampleRules.Pass()]);

        OperationOutcome<string> bad = operation.Execute(new SampleSubject(), new SampleInput("bad"));
        OperationOutcome<string> good = operation.Execute(new SampleSubject(), _input);

        bad.Errors.Select(error => error.Code).Should().Equal("BAD_INPUT");
        good.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Execute_LetsARuleFactoryUseTheTimeOfTheInput()
    {
        List<DateTimeOffset> seen = [];
        SampleOperation operation = new(
            [],
            [
                input =>
                {
                    seen.Add(input.Now);

                    return SampleRules.Pass();
                },
            ]);

        operation.Execute(new SampleSubject(), _input);

        seen.Should().Equal(SampleInput.Instant);
    }
}
