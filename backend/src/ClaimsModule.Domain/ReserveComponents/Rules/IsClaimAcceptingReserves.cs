using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveComponents.Rules;

internal sealed class IsClaimAcceptingReserves(ClaimState claimState) : IRule<object>
{
    public const string Code = "RESERVE_CLAIM_NOT_EDITABLE";

    private const string Message = "Reserves cannot change on a closed or withdrawn claim.";
    private const string Field = "ClaimId";

    public CheckResult Check(object subject)
    {
        bool isAcceptingReserves = claimState.IsEditable;

        return isAcceptingReserves
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
