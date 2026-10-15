using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

internal sealed class IsNotSelfApproval(Guid actorId) : IRule<ReserveChange>
{
    public const string Code = "S:BR-R-03";

    private const string Message = "Self-approval is not permitted.";
    private const string Field = "ActorId";

    public CheckResult<DomainFinding> Check(ReserveChange reserve)
    {
        bool isNotSelfApproval = reserve.SubmittedBy != actorId;

        return isNotSelfApproval
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
