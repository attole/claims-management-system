using ClaimsModule.Domain.ClaimDocuments.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.ClaimDocuments.Rules;

public sealed class IsFileSizeAllowedTests
{
    [Fact]
    [Trait("Rule", IsFileSizeAllowed.Code)]
    public void Check_AtTheLimit_Passes()
    {
        var rule = new IsFileSizeAllowed(IsFileSizeAllowed.MaxBytes);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsFileSizeAllowed.Code)]
    public void Check_OneByteOverTheLimit_ReturnsTheFinding()
    {
        var rule = new IsFileSizeAllowed(IsFileSizeAllowed.MaxBytes + 1);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsFileSizeAllowed.Code);
    }
}
