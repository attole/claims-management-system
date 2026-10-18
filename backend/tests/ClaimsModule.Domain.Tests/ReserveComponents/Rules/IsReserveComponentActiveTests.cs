using ClaimsModule.Domain.ReserveComponents.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.Rules;

public sealed class IsReserveComponentActiveTests
{
    [Fact]
    [Trait("Rule", IsReserveComponentActive.Code)]
    public void Check_ForAnActiveComponent_Passes()
    {
        var component = TestReserves.Component();
        var rule = new IsReserveComponentActive();

        var result = rule.Check(component);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReserveComponentActive.Code)]
    public void Check_ForAClosedComponent_ReturnsTheFinding()
    {
        var component = TestReserves.Component();
        component.Close();
        var rule = new IsReserveComponentActive();

        var result = rule.Check(component);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsReserveComponentActive.Code);
    }
}
