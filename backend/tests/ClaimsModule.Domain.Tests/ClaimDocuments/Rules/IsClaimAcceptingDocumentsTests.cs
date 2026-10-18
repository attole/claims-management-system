using ClaimsModule.Domain.ClaimDocuments.Rules;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.ClaimDocuments.Rules;

public sealed class IsClaimAcceptingDocumentsTests
{
    [Fact]
    [Trait("Rule", IsClaimAcceptingDocuments.Code)]
    public void Check_ForAnEditableClaim_Passes()
    {
        var rule = new IsClaimAcceptingDocuments(new ClaimOpenState());

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsClaimAcceptingDocuments.Code)]
    public void Check_ForAWithdrawnClaim_ReturnsTheFinding()
    {
        var rule = new IsClaimAcceptingDocuments(new ClaimWithdrawnState());

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsClaimAcceptingDocuments.Code);
    }
}
