using ClaimsModule.Domain.Claims.Issues;

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
}
