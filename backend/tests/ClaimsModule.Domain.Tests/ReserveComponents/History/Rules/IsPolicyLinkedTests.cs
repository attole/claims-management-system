using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class IsPolicyLinkedTests
{
    [Fact]
    [Trait("Rule", IsPolicyLinked.Code)]
    public void Check_WhenAPolicyIsLinked_Passes()
    {
        var rule = new IsPolicyLinked(true);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsPolicyLinked.Code)]
    public void Check_WhenNoPolicyIsLinked_ReturnsTheFinding()
    {
        var rule = new IsPolicyLinked(false);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsPolicyLinked.Code);
    }

    [Fact]
    [Trait("Rule", "S:V-Messages")]
    public void Check_WhenNoPolicyIsLinked_ReturnsTheSpecMessage()
    {
        var rule = new IsPolicyLinked(false);

        var result = rule.Check(NoSubject.Value);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Message.Should().Be("No policy linked. Policy must be associated before reserves can be set.");
    }
}
