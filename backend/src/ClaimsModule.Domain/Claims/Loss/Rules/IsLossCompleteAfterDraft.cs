using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class IsLossCompleteAfterDraft(LossDetails loss, PolicyPeriod period, CauseCodes causeCodes)
    : IRule<Claim>
{
    public const string Code = "LOSS_INCOMPLETE_AFTER_DRAFT";

    public CheckResult Check(Claim claim)
    {
        bool isDraft = claim.State.IsDraft;
        var requiredPeriod = period.RequireFor(claim.PolicyId);
        var lossCheck = new HasValidLossDetails(claim.LossEvent.ReportDay, requiredPeriod, causeCodes);
        var unwaivedCritical = lossCheck.Check(loss).CriticalFindings
            .Where(finding => !claim.HasWaivedFinding(finding))
            .ToList();

        return isDraft
            ? CheckResult.Pass
            : new CheckResult(unwaivedCritical);
    }
}
