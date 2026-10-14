using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Base.Checks;

public sealed class CheckSetTests
{
    [Fact]
    public void Evaluate_WithoutChecks_Passes()
    {
        var checks = new SampleCheckSet([], []);
        var subject = new SampleSubject();
        var input = new SampleInput("text");

        var result = checks.Evaluate(subject, input);

        result.HasNoFindings.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_CollectsTheFindingsOfEveryCheckInOrder()
    {
        var checks = new SampleCheckSet([SampleRules.Fail("A"), SampleRules.Pass(), SampleRules.Warn("B")], []);
        var subject = new SampleSubject();
        var input = new SampleInput("text");

        var result = checks.Evaluate(subject, input);

        result.Findings.Select(finding => finding.Code).Should().Equal("A", "B");
    }

    [Fact]
    public void Evaluate_WhenTheInputMakesTheCheckFail_ReturnsItsFinding()
    {
        var checks = CreateChecksFailingOnBadText();
        var subject = new SampleSubject();
        var input = new SampleInput("bad");

        var result = checks.Evaluate(subject, input);

        result.Findings.Select(finding => finding.Code).Should().Equal("BAD");
    }

    [Fact]
    public void Evaluate_WhenTheInputMakesTheCheckPass_HasNoFindings()
    {
        var checks = CreateChecksFailingOnBadText();
        var subject = new SampleSubject();
        var input = new SampleInput("good");

        var result = checks.Evaluate(subject, input);

        result.HasNoFindings.Should().BeTrue();
    }

    private static SampleCheckSet CreateChecksFailingOnBadText()
    {
        Func<SampleInput, DelegateRule> checkFromInput =
            input => input.Text == "bad" ? SampleRules.Fail("BAD") : SampleRules.Pass();

        return new SampleCheckSet([], [checkFromInput]);
    }
}
