using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Flows;

public sealed class RemoveRiskObjectOperationTests
{
    [Fact]
    public void Run_MarksTheRiskObjectRemoved()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObject = claim.RiskObjects[0];

        RemoveRiskObjectOperation.Run(riskObject, NoInput.Value);

        riskObject.IsDeleted.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", IsRiskObjectNotRemoved.Code)]
    public void Run_Twice_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObject = claim.RiskObjects[0];
        RemoveRiskObjectOperation.Run(riskObject, NoInput.Value);

        var outcome = RemoveRiskObjectOperation.Run(riskObject, NoInput.Value);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsRiskObjectNotRemoved.Code);
    }
}
