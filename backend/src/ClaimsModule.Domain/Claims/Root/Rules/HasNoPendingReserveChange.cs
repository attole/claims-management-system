using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class HasNoPendingReserveChange(ClaimReserveSnapshot reserves) : IRule<Claim>
{
    public const string Code = "S:CC-01";

    private const string Message = "Claim cannot be closed - approval of every pending reserve is not satisfied.";
    private const string Field = "Reserves";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasNoPendingReserveChange = !reserves.HasPendingReserveChange;

        return hasNoPendingReserveChange
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
