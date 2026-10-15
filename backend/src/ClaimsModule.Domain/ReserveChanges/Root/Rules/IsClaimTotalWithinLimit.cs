using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

public sealed class IsClaimTotalWithinLimit(ReserveSnapshot snapshot, decimal amount) : IRule<NoSubject>
{
    public const string Code = "S:BR-R-05";

    private const string Message = "Total reserves will exceed $10,000,000. Manager override required.";
    private const string Field = "Amount";

    public CheckResult<DomainFinding> Check(NoSubject subject)
    {
        bool isWithinLimit = snapshot.ClaimApprovedTotal + amount <= IsClaimLimitCleared.ClaimLimit;
        var warning = new RuleFinding(Code, Message, Field).AsWarning();

        return isWithinLimit
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(warning);
    }
}
