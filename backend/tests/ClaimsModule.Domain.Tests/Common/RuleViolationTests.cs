using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Tests.Common;

public sealed class RuleViolationTests
{
    [Fact]
    public void WithoutAField_HasAnEmptyField()
    {
        new RuleViolation("CODE", "message").Field.Should().BeEmpty();
    }

    [Fact]
    public void WithAField_KeepsIt()
    {
        new RuleViolation("CODE", "message", "LossDate").Field.Should().Be("LossDate");
    }
}
