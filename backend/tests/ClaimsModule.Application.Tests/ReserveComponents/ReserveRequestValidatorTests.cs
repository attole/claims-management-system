using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ReserveComponents;

public sealed class ReserveRequestValidatorTests
{
    [Fact]
    public void Validate_ForANewComponent_HasNoErrors()
    {
        var validator = new ReserveRequestValidator();
        var request = new ReserveRequest(ReserveTransactionType.Add, 1000m, "reason")
        {
            ComponentType = ReserveComponentTypeKind.Expense,
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ForAnExistingComponent_HasNoErrors()
    {
        var validator = new ReserveRequestValidator();
        var request = new ReserveRequest(ReserveTransactionType.Adjust, -200m, "reason")
        {
            ComponentId = Guid.NewGuid(),
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNeitherATypeNorAnId_Fails()
    {
        var validator = new ReserveRequestValidator();
        var request = new ReserveRequest(ReserveTransactionType.Add, 1000m, "reason");

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(reserve => reserve.ComponentId);
    }

    [Fact]
    public void Validate_WithBothATypeAndAnId_Fails()
    {
        var validator = new ReserveRequestValidator();
        var request = new ReserveRequest(ReserveTransactionType.Add, 1000m, "reason")
        {
            ComponentId = Guid.NewGuid(),
            ComponentType = ReserveComponentTypeKind.Expense,
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(reserve => reserve.ComponentId);
    }

    [Fact]
    public void Validate_WithAnUnknownTransactionType_FailsOnTheField()
    {
        var validator = new ReserveRequestValidator();
        var request = new ReserveRequest((ReserveTransactionType)99, 1000m, "reason")
        {
            ComponentId = Guid.NewGuid(),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(reserve => reserve.TransactionType);
    }

    [Fact]
    public void Validate_WithATooLongReason_FailsOnTheField()
    {
        var validator = new ReserveRequestValidator();
        var request = new ReserveRequest(
            ReserveTransactionType.Add,
            1000m,
            new string('x', ReserveFieldLengths.ChangeReason + 1))
        {
            ComponentId = Guid.NewGuid(),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(reserve => reserve.ChangeReason);
    }
}
