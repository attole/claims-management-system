using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss;

public sealed class LossReferenceTests
{
    [Theory]
    [Trait("Rule", "S:BR-C-05")]
    [InlineData("WATER", true)]
    [InlineData("water", true)]
    [InlineData("FIRE", false)]
    [InlineData("", false)]
    public void IsActiveCauseCode_ComparesWithoutCase(string code, bool isExpected)
    {
        var reference = TestClaims.LossReference();

        reference.IsActiveCauseCode(code).Should().Be(isExpected);
    }

    [Fact]
    public void RequireFor_TheReferencesOwnPolicy_ReturnsTheReference()
    {
        var reference = TestClaims.LossReference();

        var result = reference.RequireFor(TestClaims.Policy);

        result.Should().BeSameAs(reference);
    }

    [Fact]
    public void RequireFor_AnotherPolicy_Throws()
    {
        var reference = TestClaims.LossReference();

        Func<LossReference> act = () => reference.RequireFor(Guid.Empty);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_WithoutActiveCodes_Throws()
    {
        Func<LossReference> act = () => new LossReference([], PolicyPeriod.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
