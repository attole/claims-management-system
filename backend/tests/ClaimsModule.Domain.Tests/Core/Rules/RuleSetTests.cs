using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Core;

namespace ClaimsModule.Domain.Tests.Core.Rules;

public sealed class RuleSetTests
{
    [Fact]
    public void Check_WithoutRules_Passes()
    {
        var rules = new RuleSet<SampleSubject, SampleInput>();

        var result = rules.Check(new SampleSubject(), new SampleInput("text"));

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    public void Check_CollectsTheFindingsOfEveryRuleInOrder()
    {
        var rules = new RuleSet<SampleSubject, SampleInput>();
        rules.Require(SampleRules.Critical("A"));
        rules.Require(SampleRules.Pass());
        rules.Require(SampleRules.Warning("B"));

        var result = rules.Check(new SampleSubject(), new SampleInput("text"));

        result.Findings.Select(finding => finding.Code).Should().Equal("A", "B");
    }

    [Fact]
    public void Check_WhenTheInputMakesTheRuleFail_ReturnsItsFinding()
    {
        var rules = CreateRulesFailingOnBadText();

        var result = rules.Check(new SampleSubject(), new SampleInput("bad"));

        result.Findings.Select(finding => finding.Code).Should().Equal("BAD");
    }

    [Fact]
    public void Check_WhenTheInputMakesTheRulePass_HasNoFindings()
    {
        var rules = CreateRulesFailingOnBadText();

        var result = rules.Check(new SampleSubject(), new SampleInput("good"));

        result.Findings.Should().BeEmpty();
    }

    private static RuleSet<SampleSubject, SampleInput> CreateRulesFailingOnBadText()
    {
        var rules = new RuleSet<SampleSubject, SampleInput>();
        rules.Require(input => input.Text == "bad" ? SampleRules.Critical("BAD") : SampleRules.Pass());

        return rules;
    }
}
