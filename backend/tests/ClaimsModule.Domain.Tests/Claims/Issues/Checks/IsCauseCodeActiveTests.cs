using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Checks;

public sealed class IsCauseCodeActiveTests
{
    [Fact]
    [Trait("Rule", IsCauseCodeActive.Code)]
    public void Check_WhenTheCodeIsActive_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var check = new IsCauseCodeActive(["WATER"]);

        var result = check.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsCauseCodeActive.Code)]
    public void Check_ComparesTheCodeWithoutCase()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var check = new IsCauseCodeActive(["water"]);

        var result = check.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsCauseCodeActive.Code)]
    public void Check_WhenTheCodeIsNotActive_ReturnsACriticalFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var check = new IsCauseCodeActive(["COL-FIRE"]);

        var result = check.Check(claim);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsCauseCodeActive.Code);
        finding.IsBlocking.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", IsCauseCodeActive.Code)]
    public void Check_WhenNoCodeIsGiven_ReturnsTheFinding()
    {
        var claim = TestClaims.Empty();
        var check = new IsCauseCodeActive(["WATER"]);

        var result = check.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsCauseCodeActive.Code);
    }
}
