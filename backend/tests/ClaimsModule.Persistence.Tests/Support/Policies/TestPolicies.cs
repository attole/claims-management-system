using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Persistence.Tests.Support.Policies;

internal static class TestPolicies
{
    public static Policy Add(ClaimsDbContext context, Guid organisationId, string policyNumber, DateOnly? cancelledDate)
    {
        var policy = new Policy(Ids.Next(), organisationId, policyNumber, "Acme Ltd", PolicyPeriod.Empty, []);
        context.Policies.Add(policy);
        var entry = context.Entry(policy);
        entry.Property("EffectiveDate").CurrentValue = new DateOnly(2026, 1, 1);
        entry.Property("ExpirationDate").CurrentValue = new DateOnly(2026, 12, 31);
        entry.Property("CancelledDate").CurrentValue = cancelledDate;

        return policy;
    }
}
