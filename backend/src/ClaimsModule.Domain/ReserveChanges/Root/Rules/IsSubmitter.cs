using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

public sealed class IsSubmitter(Guid actorId) : IRule<ReserveChange>
{
    public const string Code = "S:API-POST-reserve-retract";

    private const string Message = "Only the submitter can retract this reserve.";
    private const string Field = "ActorId";

    public CheckResult<DomainFinding> Check(ReserveChange reserve)
    {
        bool isSubmitter = reserve.SubmittedBy == actorId;

        return isSubmitter
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
