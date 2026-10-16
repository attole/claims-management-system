using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class HasRejectionReasonTests
{
    [Fact]
    [Trait("Rule", HasRejectionReason.Code)]
    public void Check_WithAReason_Passes()
    {
        var change = TestReserves.Submit(50_000m);
        var rule = new HasRejectionReason("Not supported by the report");

        var result = rule.Check(change);

        result.Findings.Should().BeEmpty();
    }

    [Theory]
    [Trait("Rule", HasRejectionReason.Code)]
    [InlineData("")]
    [InlineData("   ")]
    public void Check_WithoutAReason_ReturnsTheFinding(string text)
    {
        var change = TestReserves.Submit(50_000m);
        var rule = new HasRejectionReason(text);

        var result = rule.Check(change);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasRejectionReason.Code);
    }
}
