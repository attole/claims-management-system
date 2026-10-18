using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ClaimDocuments.Rules;

internal sealed class IsClaimAcceptingDocuments(ClaimState claimState) : IRule<object>
{
    public const string Code = "DOCUMENT_CLAIM_NOT_EDITABLE";

    private const string Message = "Documents cannot change on a closed or withdrawn claim.";
    private const string Field = "ClaimId";

    public CheckResult Check(object subject)
    {
        bool isAcceptingDocuments = claimState.IsEditable;

        return isAcceptingDocuments
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
