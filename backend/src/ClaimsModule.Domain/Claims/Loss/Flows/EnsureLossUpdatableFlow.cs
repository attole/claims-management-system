using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Flows;

internal sealed class EnsureLossUpdatableFlow
    : Flow<EnsureLossUpdatableFlow, Claim, UpdateLossDetailsInput, Success, DomainFinding>
{
    public EnsureLossUpdatableFlow()
    {
        Require(new IsClaimEditable());
    }

    protected override Success Apply(Claim claim, UpdateLossDetailsInput input, CheckResult<DomainFinding> result)
    {
        return Success.Value;
    }
}
