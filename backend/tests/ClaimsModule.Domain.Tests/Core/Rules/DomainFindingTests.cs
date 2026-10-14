using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Core.Rules;

public sealed class DomainFindingTests
{
    [Fact]
    public void Finding_BlocksByDefault()
    {
        var finding = new RuleFinding("CODE", "message");

        finding.IsBlocking.Should().BeTrue();
    }

    [Fact]
    public void AsWarning_KeepsTheContentAndDoesNotBlock()
    {
        var finding = new RuleFinding("CODE", "message", "Field");

        var result = finding.AsWarning();

        result.IsBlocking.Should().BeFalse();
        result.Code.Should().Be("CODE");
        result.Message.Should().Be("message");
    }

    [Fact]
    public void AsWarning_LeavesTheOriginalBlocking()
    {
        var finding = new RuleFinding("CODE", "message", "Field");

        finding.AsWarning();

        finding.IsBlocking.Should().BeTrue();
    }
}
