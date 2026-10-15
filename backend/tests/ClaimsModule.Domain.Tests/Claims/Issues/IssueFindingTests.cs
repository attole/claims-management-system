using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Claims.Issues;

public sealed class IssueFindingTests
{
    [Fact]
    public void Critical_Blocks()
    {
        var result = IssueFinding.Critical("CODE", "message", "Field");

        result.IsBlocking.Should().BeTrue();
    }

    [Fact]
    public void Warning_DoesNotBlock()
    {
        var result = IssueFinding.Warning("CODE", "message", "Field");

        result.IsBlocking.Should().BeFalse();
    }

    [Fact]
    public void From_ABlockingFinding_IsCriticalWithTheSameCodeMessageAndField()
    {
        var finding = new RuleFinding("CODE", "message", "Field");

        var issue = IssueFinding.From(finding);

        issue.Severity.IsBlockingOpening.Should().BeTrue();
        issue.Should().Be(IssueFinding.Critical("CODE", "message", "Field"));
    }

    [Fact]
    public void From_AnAdvisoryFinding_IsAWarning()
    {
        var finding = new RuleFinding("CODE", "message", "Field").AsWarning();

        var issue = IssueFinding.From(finding);

        issue.Should().Be(IssueFinding.Warning("CODE", "message", "Field"));
    }
}
