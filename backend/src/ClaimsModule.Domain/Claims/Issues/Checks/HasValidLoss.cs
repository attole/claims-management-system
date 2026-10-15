using ClaimsModule.Domain.Claims.Loss.Flows;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

internal sealed class HasValidLoss(DateTimeOffset now, ClaimReferenceSnapshot reference) : ICheck<Claim, DomainFinding>
{
    public const string Code = "CLAIM_LOSS_VALID";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        var loss = claim.LossEvent;
        var evaluation = new LossEvaluationInput(now, loss.ReportDate, reference);

        return EvaluateLossFlow.Execute(loss.Details, evaluation).Result;
    }
}
