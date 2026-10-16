using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class IsClaimEditable : IRule<Claim>
{
    public const string Code = "CLAIM_NOT_EDITABLE";

    private const string Message = "A closed or withdrawn claim cannot be edited.";
    private const string Field = "Status";

    public CheckResult Check(Claim claim)
    {
        bool isEditable = claim.State.IsEditable;

        return isEditable
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
