using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ClaimDocuments.Rules;

internal sealed class IsClaimDocumentNotRemoved : IRule<ClaimDocument>
{
    public const string Code = "DOCUMENT_REMOVED";

    private const string Message = "The document has been removed.";
    private const string Field = "ClaimDocumentId";

    public CheckResult Check(ClaimDocument document)
    {
        bool isNotRemoved = !document.IsDeleted;

        return isNotRemoved
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
