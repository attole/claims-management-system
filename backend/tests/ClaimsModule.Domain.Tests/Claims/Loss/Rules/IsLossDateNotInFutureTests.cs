using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class IsLossDateNotInFutureTests
{
    [Theory]
    [Trait("Rule", IsLossDateNotInFuture.Code)]
    [Trait("Rule", "A:BR-C-01")]
    [InlineData(-24)]
    [InlineData(0)]
    public void Check_ForALossUpToNow_Passes(int hoursAfterNow)
    {
        var loss = TestClaims.Loss(TestClaims.Start.AddHours(hoursAfterNow), TestClaims.Description);

        var result = new IsLossDateNotInFuture(TestClaims.Start).Check(loss);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossDateNotInFuture.Code)]
    [Trait("Rule", "A:BR-C-01")]
    public void Check_ForALossAfterNow_ReturnsACriticalFindingOnTheLossDate()
    {
        var loss = TestClaims.Loss(TestClaims.Start.AddHours(1), TestClaims.Description);

        var result = new IsLossDateNotInFuture(TestClaims.Start).Check(loss);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsLossDateNotInFuture.Code);
        finding.Severity.Should().Be(FindingSeverity.Critical);
        finding.Field.Should().Be("LossDate");
    }

    [Fact]
    [Trait("Rule", "S:V-Messages")]
    public void Check_ForALossAfterNow_ReturnsTheSpecMessage()
    {
        var loss = TestClaims.Loss(TestClaims.Start.AddHours(1), TestClaims.Description);

        var result = new IsLossDateNotInFuture(TestClaims.Start).Check(loss);

        result.Findings.Should().ContainSingle().Which.Message.Should().Be("Loss date cannot be in the future.");
    }
}
