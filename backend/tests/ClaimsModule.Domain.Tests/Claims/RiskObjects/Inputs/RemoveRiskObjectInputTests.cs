using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Inputs;

public sealed class RemoveRiskObjectInputTests
{
    [Fact]
    public void Now_WhenNotGiven_Throws()
    {
        Func<RemoveRiskObjectInput> act = () =>
            new RemoveRiskObjectInput(Guid.NewGuid(), default, TestClaims.LossReference());

        act.Should().Throw<ArgumentException>();
    }
}
