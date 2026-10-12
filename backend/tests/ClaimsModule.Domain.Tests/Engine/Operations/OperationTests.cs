using ClaimsModule.Domain.Engine.Operations;
using ClaimsModule.Domain.Engine.Rules;
using ClaimsModule.Domain.Tests.Support.Engine;

namespace ClaimsModule.Domain.Tests.Engine.Operations;

public sealed class OperationTests
{
    private static readonly DateTimeOffset _start = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
    private static readonly SampleInput _input = new("text");

    [Fact]
    public void Execute_WhenEveryRulePasses_AppliesAndReturnsTheValue()
    {
        SampleOperation operation = new([SampleRules.Pass(), SampleRules.Pass()], []);
        SampleSubject subject = new();

        OperationOutcome<string> outcome = operation.Execute(subject, _input, Clock());

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("text");
        subject.Log.Should().Equal("apply text");
    }

    [Fact]
    public void Execute_CollectsEveryErrorAndDoesNotApply()
    {
        SampleOperation operation = new([SampleRules.Fail("A"), SampleRules.Pass(), SampleRules.Fail("B")], []);
        SampleSubject subject = new();

        OperationOutcome<string> outcome = operation.Execute(subject, _input, Clock());

        outcome.IsSuccess.Should().BeFalse();
        outcome.Errors.Select(error => error.Code).Should().Equal("A", "B");
        operation.ApplyCount.Should().Be(0);
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void Execute_KeepsWarningsOnSuccess()
    {
        SampleOperation operation = new([SampleRules.Warn("W")], []);

        OperationOutcome<string> outcome = operation.Execute(new SampleSubject(), _input, Clock());

        outcome.IsSuccess.Should().BeTrue();
        outcome.Warnings.Select(warning => warning.Code).Should().Equal("W");
    }

    [Fact]
    public void Execute_ReportsWarningsAlongsideErrors()
    {
        SampleOperation operation = new([SampleRules.Warn("W"), SampleRules.Fail("E")], []);

        OperationOutcome<string> outcome = operation.Execute(new SampleSubject(), _input, Clock());

        outcome.Errors.Select(error => error.Code).Should().Equal("E");
        outcome.Warnings.Select(warning => warning.Code).Should().Equal("W");
    }

    [Fact]
    public void Execute_BuildsARuleFromTheInput()
    {
        SampleOperation operation = new([], [input => input.Text == "bad" ? SampleRules.Fail("BAD_INPUT") : SampleRules.Pass()]);

        OperationOutcome<string> bad = operation.Execute(new SampleSubject(), new SampleInput("bad"), Clock());
        OperationOutcome<string> good = operation.Execute(new SampleSubject(), _input, Clock());

        bad.Errors.Select(error => error.Code).Should().Equal("BAD_INPUT");
        good.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Execute_ReadsTheTimeOnceAndSharesIt()
    {
        List<DateTimeOffset> seen = [];
        DelegateRule recorder = new((_, context) =>
        {
            seen.Add(context.Now);

            return RuleResult.Pass;
        });
        SampleOperation operation = new([recorder, recorder], []);
        CountingTimeProvider time = new(_start);

        operation.Execute(new SampleSubject(), _input, time);

        time.Calls.Should().Be(1);
        seen.Should().HaveCount(2).And.OnlyContain(now => now == _start);
    }

    private static CountingTimeProvider Clock()
    {
        return new CountingTimeProvider(_start);
    }
}
