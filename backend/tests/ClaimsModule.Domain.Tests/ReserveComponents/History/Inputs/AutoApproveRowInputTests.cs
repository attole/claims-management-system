using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Inputs;

public sealed class AutoApproveRowInputTests
{
    [Fact]
    public void ReviewerId_IsNobody()
    {
        var input = new AutoApproveRowInput(0m, 0m, TestReserves.Submitter, TestClaims.Later);

        input.ReviewerId.Should().Be(Guid.Empty);
    }
}
