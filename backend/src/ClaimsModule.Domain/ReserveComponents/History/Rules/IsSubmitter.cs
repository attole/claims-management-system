using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Rules;

internal sealed class IsSubmitter(Guid actorId) : IRule<ReserveHistory>
{
    public const string Code = "S:API-POST-reserve-retract";

    private const string Message = "Only the submitter can retract this reserve.";
    private const string Field = "ActorId";

    public CheckResult Check(ReserveHistory history)
    {
        bool isSubmitter = history.IsSubmittedBy(actorId);

        return isSubmitter
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
