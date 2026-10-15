using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

public sealed class HasRejectionReason(string reason) : IRule<ReserveChange>
{
    public const string Code = "RESERVE_REJECTION_REASON_REQUIRED";

    private const string Message = "A rejection reason is required.";
    private const string Field = "Reason";

    public CheckResult<DomainFinding> Check(ReserveChange reserve)
    {
        bool hasReason = !string.IsNullOrWhiteSpace(reason);

        return hasReason
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
