using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss;

public sealed class LossDetailsTests
{
    [Theory]
    [Trait("Rule", "S:BR-C-07")]
    [InlineData(19, false)]
    [InlineData(20, true)]
    public void HasLongEnoughDescription_IncludesTheMinimumLength(int length, bool isExpected)
    {
        var loss = TestClaims.Loss(TestClaims.Start, new string('a', length));

        loss.HasLongEnoughDescription.Should().Be(isExpected);
    }

    [Fact]
    public void HasLongEnoughDescription_DoesNotCountSurroundingWhitespace()
    {
        string padded = "  " + new string('a', LossDetails.MinimumDescriptionLength - 1) + "  ";
        var loss = TestClaims.Loss(TestClaims.Start, padded);

        loss.HasLongEnoughDescription.Should().BeFalse();
    }

    [Fact]
    public void HasLongEnoughDescription_ForAnEmptyLoss_IsFalse()
    {
        LossDetails.Empty.HasLongEnoughDescription.Should().BeFalse();
    }
}
