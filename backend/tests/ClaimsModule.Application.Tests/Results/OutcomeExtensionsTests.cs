using ClaimsModule.Application.Results;
using ClaimsModule.Application.Tests.Support.Claims;

namespace ClaimsModule.Application.Tests.Results;

public sealed class OutcomeExtensionsTests
{
    [Fact]
    public void ToResult_ForASucceededOutcome_HoldsTheValue()
    {
        var outcome = ApplicationClaims.Created();

        var result = outcome.ToResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(outcome.Value);
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void ToResult_ForASucceededOutcomeWithWarnings_CarriesThemOnTheResult()
    {
        var outcome = ApplicationClaims.WithdrawnWithOpenBalance();

        var result = outcome.ToResult();

        result.IsSuccess.Should().BeTrue();
        result.Warnings.Should().NotBeEmpty();
        result.Warnings.Select(warning => warning.Code).Should().Equal(
            outcome.WarningFindings.Select(finding => finding.Code));
    }

    [Fact]
    public void ToResult_ForAFailedOutcome_HoldsAValidationErrorPerCriticalFinding()
    {
        var outcome = ApplicationClaims.OpenedWithoutDetails();

        var result = outcome.ToResult();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().OnlyContain(error => error.Kind == ErrorKind.Validation);
        result.Errors.Select(error => error.Code).Should().Equal(
            outcome.CriticalFindings.Select(finding => finding.Code));
    }
}
