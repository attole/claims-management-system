using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.Rules;

namespace ClaimsModule.Domain.Tests.ReserveComponents.Rules;

public sealed class IsComponentTypeKnownTests
{
    [Theory]
    [Trait("Rule", IsComponentTypeKnown.Code)]
    [InlineData(ReserveComponentTypeKind.Indemnity)]
    [InlineData(ReserveComponentTypeKind.SubrogationRecoverable)]
    public void Check_ForADefinedType_Passes(ReserveComponentTypeKind kind)
    {
        var rule = new IsComponentTypeKnown(kind);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsComponentTypeKnown.Code)]
    public void Check_ForAnUndefinedType_ReturnsTheFindingOnTheComponentType()
    {
        var rule = new IsComponentTypeKnown((ReserveComponentTypeKind)99);

        var result = rule.Check(NoSubject.Value);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsComponentTypeKnown.Code);
        finding.Field.Should().Be("ComponentType");
    }

    [Fact]
    [Trait("Rule", "S:V-Messages")]
    public void Check_ForAnUndefinedType_ReturnsTheSpecMessage()
    {
        var rule = new IsComponentTypeKnown((ReserveComponentTypeKind)99);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Message.Should().Be("Invalid reserve component type.");
    }
}
