using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class IsNotSelfApproval(Guid actorId) : IRule<ReserveChange>
{
    public const string Code = "S:BR-R-03";

    private const string Message = "Self-approval is not permitted.";
    private const string Field = "ActorId";

    public CheckResult Check(ReserveChange change)
    {
        bool isNotSelfApproval = !change.IsSubmittedBy(actorId);

        return isNotSelfApproval
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
