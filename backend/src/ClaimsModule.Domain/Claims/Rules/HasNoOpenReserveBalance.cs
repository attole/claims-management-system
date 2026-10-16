using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoOpenReserveBalance(bool hasOpenReserveBalance) : IRule<object>
{
    public const string Code = "S:CC-04";

    private const string Message = "The claim is closed with an open reserve balance.";
    private const string Field = "Reserves";

    public CheckResult Check(object subject)
    {
        bool hasNoOpenReserveBalance = !hasOpenReserveBalance;
        var warning = new Finding(Code, Message, Field).AsWarning();

        return hasNoOpenReserveBalance
            ? CheckResult.Pass
            : CheckResult.Found(warning);
    }
}
