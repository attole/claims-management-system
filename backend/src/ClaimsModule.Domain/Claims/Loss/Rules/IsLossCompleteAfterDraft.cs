using ClaimsModule.Domain.Claims.Loss.Flows;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class IsLossCompleteAfterDraft(ChangeLossInput change) : IRule<ClaimLossEvent>
{
    public const string Code = "LOSS_INCOMPLETE_AFTER_DRAFT";

    public CheckResult<DomainFinding> Check(ClaimLossEvent loss)
    {
        bool isDraft = change.ClaimState is ClaimStateKind.Draft;
        var evaluation = new LossEvaluationInput(change.Now, loss.ReportDate, change.Reference);
        var blocking = EvaluateLossFlow.Execute(change.Details, evaluation).Blocking;

        return isDraft
            ? CheckResult<DomainFinding>.Pass
            : new CheckResult<DomainFinding>(blocking);
    }
}
