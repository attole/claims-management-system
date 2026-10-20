using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class IsDescriptionLongEnoughTests
{
    [Fact]
    [Trait("Rule", IsDescriptionLongEnough.Code)]
    public void Check_AtTheMinimumLength_Passes()
    {
        var loss = TestClaims.Loss(TestClaims.Start, new string('a', LossDetails.MinimumDescriptionLength));

        var result = new IsDescriptionLongEnough().Check(loss);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsDescriptionLongEnough.Code)]
    public void Check_ForAShortDescription_ReturnsTheFindingWithTheTrimmedText()
    {
        var loss = TestClaims.Loss(TestClaims.Start, "  Leak  ");

        var result = new IsDescriptionLongEnough().Check(loss);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsDescriptionLongEnough.Code);
        finding.Evidence.Should().Be("Leak");
    }

    [Fact]
    [Trait("Rule", "S:V-Messages")]
    public void Check_ForAShortDescription_ReturnsTheSpecMessage()
    {
        var loss = TestClaims.Loss(TestClaims.Start, "Leak");

        var result = new IsDescriptionLongEnough().Check(loss);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Message.Should().Be("Loss description is required and must be at least 20 characters.");
    }
}
