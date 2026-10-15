using ClaimsModule.Domain.Claims.Loss.Checks;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Flows;

internal sealed class EvaluateLossFlow
    : Flow<EvaluateLossFlow, LossDetails, LossEvaluationInput, Success, DomainFinding>
{
    public EvaluateLossFlow()
    {
        Require(new IsLossDatePresent());
        Require(input => new IsLossDateNotInFuture(input.Now));
        Require(input => new IsLossDateNotAfterReport(input.ReportDate));
        Require(new IsDescriptionLongEnough());
        Require(input => new IsCauseCodeActive(input.Reference.ActiveCauseCodes));
        Require(input => new IsLossDateWithinPolicyPeriod(input.Reference.PolicyPeriod));
    }

    protected override FlowDecision WhenBlocked => FlowDecision.Continue;

    protected override Success Apply(LossDetails loss, LossEvaluationInput input, CheckResult<DomainFinding> result)
    {
        return Success.Value;
    }
}
