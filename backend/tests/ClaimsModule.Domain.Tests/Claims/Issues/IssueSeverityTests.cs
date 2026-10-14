using ClaimsModule.Domain.Claims.Issues;

namespace ClaimsModule.Domain.Tests.Claims.Issues;

public sealed class IssueSeverityTests
{
    private static readonly IssueSeverity[] _severities = [new IssueCriticalSeverity(), new IssueWarningSeverity()];

    [Fact]
    [Trait("Rule", "S:VAL-Severity")]
    public void Severities_OnlyTheCriticalOneBlocksOpening()
    {
        _severities.Where(severity => severity.IsBlockingOpening).Should().ContainSingle().Which.Should()
            .Be(new IssueCriticalSeverity());
    }

    [Fact]
    public void Severities_CoverEveryKind()
    {
        _severities.Select(severity => severity.Marker).Should().BeEquivalentTo(Enum.GetValues<IssueSeverityKind>());
    }
}
