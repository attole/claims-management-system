using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations.Creation;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasOrganisationTests
{
    [Fact]
    [Trait("Rule", HasOrganisation.Code)]
    public void Check_FailsForAnEmptyOrganisation()
    {
        var result = new HasOrganisation(Guid.Empty).Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasOrganisation.Code);
    }

    [Fact]
    [Trait("Rule", HasOrganisation.Code)]
    public void Check_PassesForAnOrganisation()
    {
        var result = new HasOrganisation(Guid.NewGuid()).Check(NoSubject.Value);

        result.HasNoFindings.Should().BeTrue();
    }
}
