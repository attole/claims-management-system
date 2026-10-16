using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Rules;

public sealed class HasNoOtherPrimaryRiskObjectTests
{
    [Fact]
    [Trait("Rule", HasNoOtherPrimaryRiskObject.Code)]
    public void Check_WhenTheNewOneIsNotPrimary_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        TestClaims.AddPrimaryRiskObject(claim);

        var result = new HasNoOtherPrimaryRiskObject(Guid.Empty, isPrimary: false).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoOtherPrimaryRiskObject.Code)]
    public void Check_WhenAnotherIsPrimary_Fails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        TestClaims.AddPrimaryRiskObject(claim);

        var result = new HasNoOtherPrimaryRiskObject(Guid.Empty, isPrimary: true).Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoOtherPrimaryRiskObject.Code);
    }
}
