using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Core.Rules;

public sealed class CheckResultTests
{
    [Fact]
    public void Pass_HasNoFindingsAndNoCritical()
    {
        var result = CheckResult.Pass;

        result.Findings.Should().BeEmpty();
        result.HasCriticalFindings.Should().BeFalse();
    }

    [Fact]
    public void Found_WithACriticalFinding_HasCriticalFindings()
    {
        var result = CheckResult.Found(new Finding("A", "a"));

        result.Findings.Should().NotBeEmpty();
        result.HasCriticalFindings.Should().BeTrue();
    }

    [Fact]
    public void Found_WithAWarning_HasNoCritical()
    {
        var result = CheckResult.Found(new Finding("W", "w").AsWarning());

        result.Findings.Should().NotBeEmpty();
        result.HasCriticalFindings.Should().BeFalse();
    }

    [Fact]
    public void Combine_MergesEveryFindingAndSplitsThemBySeverity()
    {
        var first = new Finding("A", "a");
        var second = new Finding("B", "b");
        var warning = new Finding("W", "w").AsWarning();

        CheckResult[] results =
        [
            CheckResult.Found(first),
            CheckResult.Found(warning),
            CheckResult.Found(second),
        ];

        var result = CheckResult.Combine(results);

        result.CriticalFindings.Should().Equal(first, second);
        result.WarningFindings.Should().Equal(warning);
    }

    [Fact]
    public void Combine_OfNothing_IsEmpty()
    {
        var result = CheckResult.Combine([]);

        result.Findings.Should().BeEmpty();
    }
}
