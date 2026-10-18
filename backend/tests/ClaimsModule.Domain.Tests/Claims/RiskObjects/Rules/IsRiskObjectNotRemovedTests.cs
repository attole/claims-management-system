using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Rules;

public sealed class IsRiskObjectNotRemovedTests
{
    [Fact]
    [Trait("Rule", IsRiskObjectNotRemoved.Code)]
    public void Check_ForAnActiveRiskObject_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new IsRiskObjectNotRemoved().Check(claim.RiskObjects[0]);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsRiskObjectNotRemoved.Code)]
    public void Check_ForARemovedRiskObject_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObject = claim.RiskObjects[0];
        var input = new RemoveRiskObjectInput(riskObject.Id, TestClaims.Later, TestClaims.Period(), TestClaims.Codes());
        RemoveRiskObjectOnClaimOperation.Run(claim, input);

        var result = new IsRiskObjectNotRemoved().Check(riskObject);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsRiskObjectNotRemoved.Code);
    }
}
