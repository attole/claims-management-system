using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.Policies;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class PolicyConfigurationTests
{
    [Fact]
    public void Configure_CancellationDate_IsStoredAsGiven()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        TestPolicies.Add(context, TestDatabase.OrganisationId, "POL-1", new DateOnly(2026, 6, 30));
        context.SaveChanges();

        var result = context.Policies
            .Select(policy => EF.Property<DateOnly?>(policy, "CancelledDate"))
            .Single();

        result.Should().Be(new DateOnly(2026, 6, 30));
    }

    [Fact]
    public void Configure_MissingCancellationDate_IsStoredAsNull()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        TestPolicies.Add(context, TestDatabase.OrganisationId, "POL-1", null);
        context.SaveChanges();

        var result = context.Policies
            .Select(policy => EF.Property<DateOnly?>(policy, "CancelledDate"))
            .Single();

        result.Should().BeNull();
    }
}
