using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class IsClaimEditable : IRule<Claim>
{
    public const string Code = "CLAIM_NOT_EDITABLE";

    private const string Message = "A closed or withdrawn claim cannot be edited.";
    private const string Field = "Status";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool isEditable = claim.State.Marker is not (ClaimStateKind.Closed or ClaimStateKind.Withdrawn);

        return isEditable
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
