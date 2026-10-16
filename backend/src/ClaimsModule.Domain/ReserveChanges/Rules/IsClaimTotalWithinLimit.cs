using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class IsClaimTotalWithinLimit(decimal claimApprovedTotal, decimal amount) : IRule<object>
{
    public const string Code = "S:BR-R-05";

    private const string Message = "Total reserves will exceed $10,000,000. Manager override required.";
    private const string Field = "Amount";

    public CheckResult Check(object subject)
    {
        bool isWithinLimit = Claim.IsReserveChangeWithinLimit(claimApprovedTotal, amount);
        var warning = new Finding(Code, Message, Field).AsWarning();

        return isWithinLimit
            ? CheckResult.Pass
            : CheckResult.Found(warning);
    }
}
