using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.ClaimDocuments.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.ClaimDocuments.Flows;

internal sealed class RemoveClaimDocumentOperation
    : Operation<RemoveClaimDocumentOperation, ClaimDocument, RemoveClaimDocumentInput, ClaimDocument>
{
    public RemoveClaimDocumentOperation()
    {
        Require(input => new IsClaimAcceptingDocuments(input.ClaimState));
        Require(new IsClaimDocumentNotRemoved());
    }

    protected override Outcome<ClaimDocument> Apply(ClaimDocument document, RemoveClaimDocumentInput input)
    {
        document.IsDeleted = true;

        return Done(document);
    }
}
