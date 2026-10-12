using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Tests.Engine.Operations;

public sealed class OperationOutcomeTests
{
    [Fact]
    public void Success_CarriesTheValueAndWarnings()
    {
        RuleViolation warning = new("W", "w");

        OperationOutcome<string> outcome = OperationOutcome<string>.Success("done", [warning]);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be("done");
        outcome.Errors.Should().BeEmpty();
        outcome.Warnings.Should().Equal(warning);
    }

    [Fact]
    public void Failure_CarriesTheErrorsAndHasNoValue()
    {
        RuleViolation error = new("E", "e");

        OperationOutcome<string> outcome = OperationOutcome<string>.Failure([error], []);

        outcome.IsSuccess.Should().BeFalse();
        outcome.Errors.Should().Equal(error);
        Func<string> read = () => outcome.Value;
        read.Should().Throw<InvalidOperationException>();
    }
}
