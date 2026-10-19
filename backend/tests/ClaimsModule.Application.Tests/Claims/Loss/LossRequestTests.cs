using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Loss;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Loss;

public sealed class LossRequestTests
{
    [Fact]
    public void ToDetails_WithoutAnEstimate_LeavesTheEstimateUnset()
    {
        var request = ApplicationRequests.Loss();

        var result = request.ToDetails();

        result.EstimatedLossAmount.IsSet.Should().BeFalse();
    }

    [Fact]
    public void ToDetails_WithAnEstimate_SetsIt()
    {
        var request = ApplicationRequests.Loss() with { EstimatedLossAmount = 2500m };

        var result = request.ToDetails();

        result.EstimatedLossAmount.Value.Should().Be(2500m);
    }

    [Fact]
    public void ToDetails_WithNullOptionalText_UsesEmptyText()
    {
        var request = ApplicationRequests.Loss() with
        {
            Location = null!,
            PoliceReportNumber = null!,
        };

        var result = request.ToDetails();

        result.Location.Should().BeEmpty();
        result.PoliceReportNumber.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithoutAnEstimate_HasNoErrors()
    {
        var validator = new LossRequestValidator();
        var request = ApplicationRequests.Loss();

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithANegativeEstimate_FailsOnTheEstimate()
    {
        var validator = new LossRequestValidator();
        var request = ApplicationRequests.Loss() with { EstimatedLossAmount = -1m };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(loss => loss.EstimatedLossAmount);
    }

    [Fact]
    public void Validate_WithoutADescription_FailsOnTheDescription()
    {
        var validator = new LossRequestValidator();
        var request = ApplicationRequests.Loss() with { Description = null! };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(loss => loss.Description);
    }

    [Fact]
    public void Validate_WithATooLongCauseOfLossCode_FailsOnTheField()
    {
        var validator = new LossRequestValidator();
        var request = ApplicationRequests.Loss() with
        {
            CauseOfLossCode = new string('x', ClaimFieldLengths.CauseOfLossCode + 1),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(loss => loss.CauseOfLossCode);
    }

    [Fact]
    public void Validate_WithATooLongLocation_FailsOnTheField()
    {
        var validator = new LossRequestValidator();
        var request = ApplicationRequests.Loss() with
        {
            Location = new string('x', ClaimFieldLengths.Location + 1),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(loss => loss.Location);
    }

    [Fact]
    public void Validate_WithATooLongPoliceReportNumber_FailsOnTheField()
    {
        var validator = new LossRequestValidator();
        var request = ApplicationRequests.Loss() with
        {
            PoliceReportNumber = new string('x', ClaimFieldLengths.PoliceReportNumber + 1),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(loss => loss.PoliceReportNumber);
    }
}
