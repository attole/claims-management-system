using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class IsClaimLimitCleared(decimal claimApprovedTotal, bool hasAggregateLimitOverride)
    : IRule<ReserveChange>
{
    public const string Code = "S:BR-R-05";

    private const string Message = "Total reserves will exceed $10,000,000. Manager override required.";
    private const string Field = "Amount";

    public CheckResult Check(ReserveChange change)
    {
        bool isWithinLimit = Claim.IsReserveChangeWithinLimit(claimApprovedTotal, change.Details.Amount);
        bool isCleared = isWithinLimit || hasAggregateLimitOverride;

        return isCleared
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
