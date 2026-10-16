using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoOpenReserveBalanceOnWithdrawal(bool hasOpenReserveBalance) : IRule<object>
{
    public const string Code = "CLAIM_WITHDRAWN_WITH_OPEN_BALANCE";

    private const string Message = "The claim is withdrawn with an open reserve balance.";
    private const string Field = "Reserves";

    public CheckResult Check(object subject)
    {
        bool hasNoOpenReserveBalance = !hasOpenReserveBalance;

        return hasNoOpenReserveBalance
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field).AsWarning());
    }
}
