using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Rows;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Policies;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class PolicyCoverageTypeConfigurationTests
{
    [Fact]
    public void Configure_CoverageType_BelongsToItsPolicy()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var policy = TestPolicies.Add(context, TestDatabase.OrganisationId, "POL-1", null);
        context.PolicyCoverageTypes.Add(new PolicyCoverageType(Ids.Next(), policy.Id, policy.OrganisationId, "Theft"));
        context.SaveChanges();

        var result = context.PolicyCoverageTypes
            .Where(type => type.PolicyId == policy.Id)
            .ToList();

        result.Should().ContainSingle(type => type.Name == "Theft");
    }
}
