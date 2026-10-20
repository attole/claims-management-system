using ClaimsModule.Persistence.Schema.Seed;
using ClaimsModule.Persistence.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Schema.Seed;

public sealed class PolicySeedTests
{
    [Fact]
    [Trait("Rule", "S:POL-Seed")]
    public void Policies_AfterModelCreation_AreFivePoliciesOfTheSeedOrganisation()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = context.Policies.IgnoreQueryFilters().Select(policy => policy.OrganisationId).ToList();

        result.Should().HaveCount(5).And.AllBeEquivalentTo(SeedIds.Organisation);
    }

    [Fact]
    [Trait("Rule", "S:POL-Seed")]
    public void Policies_AfterModelCreation_HaveOneExpiredPolicy()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = context.Policies
            .IgnoreQueryFilters()
            .Where(policy => EF.Property<DateOnly>(policy, "ExpirationDate") < new DateOnly(2022, 1, 1))
            .Select(policy => policy.PolicyNumber)
            .ToList();

        result.Should().Equal("POL-2023-000099");
    }

    [Fact]
    [Trait("Rule", "S:POL-Seed")]
    public void CoverageTypes_AfterModelCreation_BelongToTheSeededPolicies()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var policyIds = context.Policies.IgnoreQueryFilters().Select(policy => policy.Id).ToList();

        var result = context.PolicyCoverageTypes.IgnoreQueryFilters().Select(type => type.PolicyId).ToList();

        result.Should().HaveCount(9).And.OnlyContain(policyId => policyIds.Contains(policyId));
    }
}
