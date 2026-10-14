using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Core.Base.Checks;

public sealed class CheckResultTests
{
    [Fact]
    public void Pass_HasNoFindingsAndDoesNotBlock()
    {
        var result = CheckResult<DomainFinding>.Pass;

        result.Findings.Should().BeEmpty();
        result.HasNoFindings.Should().BeTrue();
        result.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void Found_BlocksWhenTheFindingBlocks()
    {
        var result = CheckResult<DomainFinding>.Found(new RuleFinding("A", "a"));

        result.HasNoFindings.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void Found_DoesNotBlockWhenTheFindingIsAWarning()
    {
        var result = CheckResult<DomainFinding>.Found(new RuleFinding("W", "w").AsWarning());

        result.HasNoFindings.Should().BeFalse();
        result.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void Combine_MergesEveryFindingAndSplitsThemBySeverity()
    {
        var first = new RuleFinding("A", "a");
        var second = new RuleFinding("B", "b");
        var warning = new RuleFinding("W", "w").AsWarning();

        CheckResult<DomainFinding>[] results =
        [
            CheckResult<DomainFinding>.Found(first),
            CheckResult<DomainFinding>.Found(warning),
            CheckResult<DomainFinding>.Found(second),
        ];

        var result = CheckResult<DomainFinding>.Combine(results);

        result.Blocking.Should().Equal(first, second);
        result.Advisory.Should().Equal(warning);
    }

    [Fact]
    public void Combine_OfNothing_IsEmpty()
    {
        var result = CheckResult<DomainFinding>.Combine([]);

        result.Findings.Should().BeEmpty();
    }
}
