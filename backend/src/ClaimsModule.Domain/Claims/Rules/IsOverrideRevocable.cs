using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class IsOverrideRevocable(bool hasAggregateLimitOverride, decimal claimApprovedTotal) : IRule<object>
{
    public const string Code = "CLAIM_OVERRIDE_STILL_NEEDED";

    private const string Message = "The limit override cannot be removed while approved reserves exceed the limit.";
    private const string Field = "HasAggregateLimitOverride";

    public CheckResult Check(object subject)
    {
        bool isRevocable = hasAggregateLimitOverride || Claim.IsWithinReserveLimit(claimApprovedTotal);

        return isRevocable
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
