using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasClaimNumberTests
{
    [Fact]
    [Trait("Rule", HasClaimNumber.Code)]
    public void Check_FailsForTheEmptyClaimNumber()
    {
        var result = new HasClaimNumber(ClaimNumber.Empty).Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasClaimNumber.Code);
    }

    [Fact]
    [Trait("Rule", HasClaimNumber.Code)]
    public void Check_PassesForAClaimNumber()
    {
        var result = new HasClaimNumber(ClaimNumber.From("CLM-2026-0000001")).Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }
}
