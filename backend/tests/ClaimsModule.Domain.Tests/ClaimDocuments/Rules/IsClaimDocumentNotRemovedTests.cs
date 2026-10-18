using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.ClaimDocuments.Rules;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Tests.Support.ClaimDocuments;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.ClaimDocuments.Rules;

public sealed class IsClaimDocumentNotRemovedTests
{
    [Fact]
    [Trait("Rule", IsClaimDocumentNotRemoved.Code)]
    public void Check_ForAKeptDocument_Passes()
    {
        var document = TestDocuments.Upload();
        var rule = new IsClaimDocumentNotRemoved();

        var result = rule.Check(document);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsClaimDocumentNotRemoved.Code)]
    public void Check_ForARemovedDocument_ReturnsTheFinding()
    {
        var document = TestDocuments.Upload();
        document.Remove(new RemoveClaimDocumentInput(new ClaimOpenState(), TestClaims.Later));
        var rule = new IsClaimDocumentNotRemoved();

        var result = rule.Check(document);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsClaimDocumentNotRemoved.Code);
    }
}
