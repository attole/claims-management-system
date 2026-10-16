using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Reserves.Rules;

internal sealed class IsClaimAcceptingReserves(bool isClaimEditable) : IRule<object>
{
    public const string Code = "RESERVE_CLAIM_NOT_EDITABLE";

    private const string Message = "Reserves cannot change on a closed or withdrawn claim.";
    private const string Field = "ClaimId";

    public CheckResult Check(object subject)
    {
        bool isAcceptingReserves = isClaimEditable;

        return isAcceptingReserves
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
