using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class HasRejectionReason(string reason) : IRule<object>
{
    public const string Code = "RESERVE_REJECTION_REASON_REQUIRED";

    private const string Message = "A rejection reason is required.";
    private const string Field = "Reason";

    public CheckResult Check(object subject)
    {
        bool hasReason = !string.IsNullOrWhiteSpace(reason);

        return hasReason
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
