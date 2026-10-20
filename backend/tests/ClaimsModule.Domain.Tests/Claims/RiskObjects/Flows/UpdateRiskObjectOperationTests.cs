using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Flows;

public sealed class UpdateRiskObjectOperationTests
{
    [Fact]
    public void Run_ReplacesTheDetails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObject = claim.RiskObjects[0];
        var details = new RiskObjectDetails(AssetType.Equipment, "Pump");
        var input = new RiskObjectDetailsInput(details);

        var outcome = UpdateRiskObjectOperation.Run(riskObject, input);

        outcome.Value.Details.Should().Be(details);
    }

    [Fact]
    [Trait("Rule", IsRiskObjectNotRemoved.Code)]
    public void Run_ForARemovedRiskObject_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObject = claim.RiskObjects[0];
        RemoveRiskObjectOperation.Run(riskObject, NoInput.Value);
        var details = new RiskObjectDetails(AssetType.Equipment, "Pump");
        var input = new RiskObjectDetailsInput(details);

        var outcome = UpdateRiskObjectOperation.Run(riskObject, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsRiskObjectNotRemoved.Code);
    }
}
