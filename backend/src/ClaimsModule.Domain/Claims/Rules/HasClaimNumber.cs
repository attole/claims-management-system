using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasClaimNumber(ClaimNumber claimNumber) : IRule<object>
{
    public const string Code = "CLAIM_NUMBER_REQUIRED";

    private const string Message = "A claim needs a claim number.";
    private const string Field = "ClaimNumber";

    public CheckResult Check(object subject)
    {
        bool hasClaimNumber = !claimNumber.Equals(ClaimNumber.Empty);

        return hasClaimNumber
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
