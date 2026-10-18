using ClaimsModule.Domain.ClaimDocuments.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.ClaimDocuments.Rules;

public sealed class IsContentTypeAllowedTests
{
    [Theory]
    [Trait("Rule", IsContentTypeAllowed.Code)]
    [InlineData("application/pdf")]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("text/plain")]
    [InlineData("text/csv")]
    [InlineData("APPLICATION/PDF")]
    public void Check_ForAnAllowedType_Passes(string contentType)
    {
        var rule = new IsContentTypeAllowed(contentType);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Theory]
    [Trait("Rule", IsContentTypeAllowed.Code)]
    [InlineData("application/x-msdownload")]
    [InlineData("text/html")]
    [InlineData("")]
    public void Check_ForATypeOutsideTheAllowlist_ReturnsTheFinding(string contentType)
    {
        var rule = new IsContentTypeAllowed(contentType);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsContentTypeAllowed.Code);
    }
}
