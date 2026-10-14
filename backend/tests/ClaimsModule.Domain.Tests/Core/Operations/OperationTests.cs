using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Operations;

public sealed class OperationTests
{
    private static readonly SampleInput _input = new("text");

    [Fact]
    public void Execute_WhenEveryRulePasses_AppliesAndReturnsTheValue()
    {
        var operation = new SampleOperation([SampleRules.Pass(), SampleRules.Pass()], []);
        var subject = new SampleSubject();

        var outcome = operation.Execute(subject, _input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("text");
        subject.Log.Should().Equal("apply text");
    }

    [Fact]
    public void Execute_CollectsEveryErrorAndDoesNotApply()
    {
        var operation = new SampleOperation([SampleRules.Fail("A"), SampleRules.Pass(), SampleRules.Fail("B")], []);
        var subject = new SampleSubject();

        var outcome = operation.Execute(subject, _input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.Blocking.Select(finding => finding.Code).Should().Equal("A", "B");
        operation.ApplyCount.Should().Be(0);
        subject.Log.Should().BeEmpty();
    }

    [Fact]
    public void Execute_KeepsWarningsOnSuccess()
    {
        var operation = new SampleOperation([SampleRules.Warn("W")], []);

        var outcome = operation.Execute(new SampleSubject(), _input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Advisory.Select(finding => finding.Code).Should().Equal("W");
    }

    [Fact]
    public void Execute_ReportsWarningsAlongsideErrors()
    {
        var operation = new SampleOperation([SampleRules.Warn("W"), SampleRules.Fail("E")], []);

        var outcome = operation.Execute(new SampleSubject(), _input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal("E");
        outcome.Advisory.Select(finding => finding.Code).Should().Equal("W");
    }

    [Fact]
    public void Execute_WhenTheInputMakesARuleFromTheInputFail_ReturnsItsFinding()
    {
        var operation = CreateOperationFailingOnBadText();
        var subject = new SampleSubject();
        var input = new SampleInput("bad");

        var outcome = operation.Execute(subject, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal("BAD_INPUT");
    }

    [Fact]
    public void Execute_WhenTheInputMakesARuleFromTheInputPass_Succeeds()
    {
        var operation = CreateOperationFailingOnBadText();

        var outcome = operation.Execute(new SampleSubject(), _input);

        outcome.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Execute_PassesTheTimeOfTheInputToARuleFromTheInput()
    {
        List<DateTimeOffset> seen = [];
        var operation = new SampleOperation(
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

    private static SampleOperation CreateOperationFailingOnBadText()
    {
        Func<SampleInput, DelegateRule> ruleFromInput =
            input => input.Text == "bad" ? SampleRules.Fail("BAD_INPUT") : SampleRules.Pass();

        return new SampleOperation([], [ruleFromInput]);
    }
}
