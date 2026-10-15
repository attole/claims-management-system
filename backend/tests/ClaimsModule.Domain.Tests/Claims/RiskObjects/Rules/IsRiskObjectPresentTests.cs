using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Rules;

public sealed class IsRiskObjectPresentTests
{
    [Fact]
    [Trait("Rule", IsRiskObjectPresent.Code)]
    public void Check_PassesForARiskObjectOnTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new IsRiskObjectPresent(claim.RiskObjects[0].Id).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsRiskObjectPresent.Code)]
    public void Check_FailsForAnUnknownRiskObject()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new IsRiskObjectPresent(Guid.NewGuid()).Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsRiskObjectPresent.Code);
    }

    [Fact]
    [Trait("Rule", IsRiskObjectPresent.Code)]
    public void Check_FailsForARemovedRiskObject()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObjectId = claim.RiskObjects[0].Id;
        var input = new RemoveRiskObjectInput(riskObjectId, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());
        RemoveRiskObjectOperation.Execute(claim, input);

        var result = new IsRiskObjectPresent(riskObjectId).Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsRiskObjectPresent.Code);
    }
}
