using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Core.Rules;

public sealed class RuleFindingTests
{
    [Fact]
    public void Field_WhenNotGiven_IsEmpty()
    {
        var finding = new RuleFinding("CODE", "message");

        finding.Field.Should().BeEmpty();
    }

    [Fact]
    public void Field_WhenGiven_IsKept()
    {
        var finding = new RuleFinding("CODE", "message", "LossDate");

        finding.Field.Should().Be("LossDate");
    }
}
