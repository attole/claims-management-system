using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Core.Rules;

public sealed class FindingTests
{
    [Fact]
    public void Severity_ByDefault_IsCritical()
    {
        var finding = new Finding("CODE", "message");

        finding.Severity.Should().Be(FindingSeverity.Critical);
    }

    [Fact]
    public void Field_WhenNotGiven_IsEmpty()
    {
        var finding = new Finding("CODE", "message");

        finding.Field.Should().BeEmpty();
    }

    [Fact]
    public void Field_WhenGiven_IsKept()
    {
        var finding = new Finding("CODE", "message", "LossDate");

        finding.Field.Should().Be("LossDate");
    }

    [Fact]
    public void Evidence_WhenNotGiven_IsEmpty()
    {
        var finding = new Finding("CODE", "message");

        finding.Evidence.Should().BeEmpty();
    }

    [Fact]
    public void AsWarning_KeepsTheContentAndBecomesAWarning()
    {
        var finding = new Finding("CODE", "message", "Field") { Evidence = "2026-01-01" };

        var result = finding.AsWarning();

        result.Severity.Should().Be(FindingSeverity.Warning);
        result.Code.Should().Be("CODE");
        result.Message.Should().Be("message");
        result.Field.Should().Be("Field");
        result.Evidence.Should().Be("2026-01-01");
    }

    [Fact]
    public void AsWarning_LeavesTheOriginalCritical()
    {
        var finding = new Finding("CODE", "message", "Field");

        finding.AsWarning();

        finding.Severity.Should().Be(FindingSeverity.Critical);
    }

    [Theory]
    [InlineData("CODE", true)]
    [InlineData("code", false)]
    public void HasCode_ComparesOrdinally(string code, bool isExpected)
    {
        var finding = new Finding("CODE", "message");

        finding.HasCode(code).Should().Be(isExpected);
    }

    [Theory]
    [InlineData("CODE", "2026-01-01", true)]
    [InlineData("CODE", "2026-01-02", false)]
    [InlineData("OTHER", "2026-01-01", false)]
    public void IsSameProblem_NeedsTheSameCodeAndEvidence(string code, string evidence, bool isExpected)
    {
        var finding = new Finding("CODE", "message") { Evidence = "2026-01-01" };
        var other = new Finding(code, "other message") { Evidence = evidence };

        finding.IsSameProblem(other).Should().Be(isExpected);
    }
}
