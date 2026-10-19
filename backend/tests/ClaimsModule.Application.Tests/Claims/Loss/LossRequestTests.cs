using ClaimsModule.Application.Tests.Support.Claims;

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
}
