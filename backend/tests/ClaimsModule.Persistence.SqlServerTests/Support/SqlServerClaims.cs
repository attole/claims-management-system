using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Persistence.SqlServerTests.Support;

internal static class SqlServerClaims
{
    private static readonly CauseCodes _codes = new(["WATER"]);

    public static Claim Create(Guid organisationId)
    {
        var number = ClaimNumber.Create(2026, Random.Shared.Next(1, 9_999_999));
        var input = new CreateClaimInput(
            organisationId,
            number,
            SqlServerFixture.Now,
            SqlServerFixture.Now,
            PolicyPeriod.Empty,
            _codes);

        return Claim.Create(input).Value;
    }

    public static Claim CreateWithLoss(Guid organisationId, DateTimeOffset lossDate)
    {
        var claim = Create(organisationId);
        var today = DateOnly.FromDateTime(SqlServerFixture.Now.UtcDateTime);
        var period = new PolicyPeriod(Guid.Empty, today.AddYears(-1), today.AddYears(1), DateOnly.MinValue);
        var loss = new LossDetails(new LossDate(lossDate), "Water damage in the kitchen", "WATER");

        claim.UpdateLossDetails(new UpdateLossDetailsInput(loss, period, _codes, SqlServerFixture.Now));

        return claim;
    }

    public static Claim CreateLinked(Guid organisationId, Guid policyId)
    {
        var claim = Create(organisationId);
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2026, 12, 31);
        var period = new PolicyPeriod(policyId, start, end, DateOnly.MinValue);

        claim.LinkPolicy(new LinkPolicyInput(policyId, period, _codes, SqlServerFixture.Now));

        return claim;
    }
}
