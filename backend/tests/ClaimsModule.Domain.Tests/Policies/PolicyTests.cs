using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Policies;

public sealed class PolicyTests
{
    [Fact]
    public void Policy_KeepsTheDataItWasBuiltWith()
    {
        var period = TestClaims.Period();

        var result = new Policy(
            TestClaims.Policy,
            TestClaims.Organisation,
            "POL-2026-000001",
            "Acme Ltd",
            period,
            ["Property", "Liability"]);

        result.Id.Should().Be(TestClaims.Policy);
        result.OrganisationId.Should().Be(TestClaims.Organisation);
        result.PolicyNumber.Should().Be("POL-2026-000001");
        result.ClientName.Should().Be("Acme Ltd");
        result.Period.Should().Be(period);
        result.CoverageTypes.Should().Equal("Property", "Liability");
    }
}
