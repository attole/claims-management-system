using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

public sealed class IsClaimLimitCleared(ReserveSnapshot snapshot) : IRule<ReserveChange>
{
    public const string Code = "S:BR-R-05";
    public const decimal ClaimLimit = 10_000_000m;

    private const string Message = "Total reserves will exceed $10,000,000. Manager override required.";
    private const string Field = "Amount";

    public CheckResult<DomainFinding> Check(ReserveChange reserve)
    {
        bool isWithinLimit = snapshot.ClaimApprovedTotal + reserve.Details.Amount <= ClaimLimit;
        bool isCleared = isWithinLimit || snapshot.HasLimitOverride;

        return isCleared
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
