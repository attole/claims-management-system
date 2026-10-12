using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Rules;

namespace ClaimsModule.Domain.Tests.Engine.Rules;

public sealed class RuleResultTests
{
    [Fact]
    public void Pass_HasNoErrorsOrWarnings()
    {
        RuleResult.Pass.Errors.Should().BeEmpty();
        RuleResult.Pass.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Combine_MergesEveryErrorAndWarning()
    {
        RuleViolation first = new("A", "a");
        RuleViolation second = new("B", "b");
        RuleViolation warning = new("W", "w");

        RuleResult combined = RuleResult.Combine(
            [RuleResult.Fail(first), RuleResult.Warn(warning), RuleResult.Fail(second)]);

        combined.Errors.Should().Equal(first, second);
        combined.Warnings.Should().Equal(warning);
    }

    [Fact]
    public void Combine_OfNothing_IsEmpty()
    {
        RuleResult combined = RuleResult.Combine([]);

        combined.Errors.Should().BeEmpty();
        combined.Warnings.Should().BeEmpty();
    }
}
