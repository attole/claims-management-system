using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class HasNoOpenReserveBalance(ClaimReserveSnapshot reserves) : IRule<Claim>
{
    public const string Code = "S:CC-04";

    private const string Message = "The claim is closed with an open reserve balance.";
    private const string Field = "Reserves";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasNoOpenReserveBalance = !reserves.HasOpenReserveBalance;
        var warning = new RuleFinding(Code, Message, Field).AsWarning();

        return hasNoOpenReserveBalance
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(warning);
    }
}
