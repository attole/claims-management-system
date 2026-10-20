using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class IsCauseCodeActiveTests
{
    [Fact]
    [Trait("Rule", IsCauseCodeActive.Code)]
    public void Check_ForAnActiveCode_Passes()
    {
        var loss = TestClaims.Loss(TestClaims.Start, TestClaims.Description);

        var result = new IsCauseCodeActive(true).Check(loss);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsCauseCodeActive.Code)]
    [Trait("Rule", "A:BR-C-05")]
    public void Check_ForAnInactiveCode_ReturnsACriticalFindingWithTheCode()
    {
        var loss = TestClaims.Loss(TestClaims.Start, TestClaims.Description) with { CauseOfLossCode = "FIRE" };

        var result = new IsCauseCodeActive(false).Check(loss);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsCauseCodeActive.Code);
        finding.Severity.Should().Be(FindingSeverity.Critical);
        finding.Evidence.Should().Be("FIRE");
    }

    [Fact]
    [Trait("Rule", "S:V-Messages")]
    public void Check_ForAnInactiveCode_ReturnsTheSpecMessage()
    {
        var loss = TestClaims.Loss(TestClaims.Start, TestClaims.Description);

        var result = new IsCauseCodeActive(false).Check(loss);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Message.Should().Be("Cause of loss code is not recognised or is inactive.");
    }
}
