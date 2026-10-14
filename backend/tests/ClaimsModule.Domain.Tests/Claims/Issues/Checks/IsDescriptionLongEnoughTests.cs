using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Checks;

public sealed class IsDescriptionLongEnoughTests
{
    [Fact]
    [Trait("Rule", IsDescriptionLongEnough.Code)]
    public void Check_FailsBelowTheMinimumLength()
    {
        var claim = TestClaims.WithDescription(new string('a', IsDescriptionLongEnough.MinimumLength - 1));

        var result = new IsDescriptionLongEnough().Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsDescriptionLongEnough.Code);
    }

    [Fact]
    [Trait("Rule", IsDescriptionLongEnough.Code)]
    public void Check_PassesAtTheMinimumLength()
    {
        var claim = TestClaims.WithDescription(new string('a', IsDescriptionLongEnough.MinimumLength));

        var result = new IsDescriptionLongEnough().Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsDescriptionLongEnough.Code)]
    public void Check_DoesNotCountSurroundingWhitespace()
    {
        string padded = "  " + new string('a', IsDescriptionLongEnough.MinimumLength - 1) + "  ";
        var claim = TestClaims.WithDescription(padded);

        var result = new IsDescriptionLongEnough().Check(claim);

        result.Findings.Should().ContainSingle();
    }

    [Fact]
    [Trait("Rule", IsDescriptionLongEnough.Code)]
    public void Check_FailsForAnEmptyDescription()
    {
        var claim = TestClaims.Empty();

        var result = new IsDescriptionLongEnough().Check(claim);

        result.Findings.Should().ContainSingle();
    }
}
