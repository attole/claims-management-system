using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Inputs;

public sealed class AutoApproveReserveInputTests
{
    [Fact]
    public void ReviewerId_IsNobody()
    {
        var input = new AutoApproveReserveInput(0m, 0m, TestReserves.Submitter, TestClaims.Later);

        input.ReviewerId.Should().Be(Guid.Empty);
    }
}
