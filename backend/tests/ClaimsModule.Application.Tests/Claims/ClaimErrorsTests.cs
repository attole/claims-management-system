using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.Tests.Claims;

public sealed class ClaimErrorsTests
{
    [Fact]
    public void ClaimNotFound_IsANotFoundErrorWithItsCode()
    {
        var error = ClaimErrors.ClaimNotFound(Guid.NewGuid());

        error.Kind.Should().Be(ErrorKind.NotFound);
        error.Code.Should().Be(ClaimErrors.ClaimNotFoundCode);
    }

    [Theory]
    [InlineData(ClaimErrors.PartyNotFoundCode)]
    [InlineData(ClaimErrors.RiskObjectNotFoundCode)]
    [InlineData(ClaimErrors.IssueNotFoundCode)]
    [InlineData(ClaimErrors.PolicyNotFoundCode)]
    public void Factories_AreNotFoundErrorsWithDistinctCodes(string code)
    {
        var errors = new[]
        {
            ClaimErrors.PartyNotFound(Guid.NewGuid()),
            ClaimErrors.RiskObjectNotFound(Guid.NewGuid()),
            ClaimErrors.IssueNotFound(Guid.NewGuid()),
            ClaimErrors.PolicyNotFound(Guid.NewGuid()),
        };

        errors.Should().ContainSingle(error => error.Code == code);
        errors.Should().OnlyContain(error => error.Kind == ErrorKind.NotFound);
    }

    [Fact]
    public void StatusUnsupported_IsAValidationErrorOnTheTargetStatus()
    {
        var error = ClaimErrors.StatusUnsupported("Draft", ["Open"]);

        error.Kind.Should().Be(ErrorKind.Validation);
        error.Field.Should().Be("TargetStatus");
    }

    [Fact]
    public void WithNextStatuses_KeepsTheRefusalAndListsTheValidNextStatuses()
    {
        var refusal = Error.Validation("TRANSITION_NOT_PERMITTED", "Not permitted.");

        var error = ClaimErrors.WithNextStatuses(refusal, ["Open", "Withdrawn"]);

        error.Code.Should().Be("TRANSITION_NOT_PERMITTED");
        error.Message.Should().Be("Not permitted. Valid next statuses: Open, Withdrawn.");
        error.Field.Should().Be("TargetStatus");
    }

    [Fact]
    public void WithNextStatuses_WithoutAnyNextStatus_SaysNone()
    {
        var refusal = Error.Validation("TRANSITION_NOT_PERMITTED", "Not permitted.");

        var error = ClaimErrors.WithNextStatuses(refusal, []);

        error.Message.Should().EndWith("Valid next statuses: none.");
    }
}
