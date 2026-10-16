using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Flows;

public sealed class RemoveRiskObjectOperationTests
{
    [Fact]
    public void Run_MarksTheRiskObjectRemovedAtTheTimeOfTheInput()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObject = claim.RiskObjects[0];
        var input = new RiskObjectRemovalInput(TestClaims.Later);

        RemoveRiskObjectOperation.Run(riskObject, input);

        riskObject.IsDeleted.Should().BeTrue();
        riskObject.DeletedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", IsRiskObjectNotRemoved.Code)]
    public void Run_Twice_ReturnsTheFindingAndKeepsTheFirstTime()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObject = claim.RiskObjects[0];
        RemoveRiskObjectOperation.Run(riskObject, new RiskObjectRemovalInput(TestClaims.Later));
        var later = new RiskObjectRemovalInput(TestClaims.Later.AddHours(1));

        var outcome = RemoveRiskObjectOperation.Run(riskObject, later);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsRiskObjectNotRemoved.Code);
        riskObject.DeletedAt.Should().Be(TestClaims.Later);
    }
}
