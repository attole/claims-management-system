using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.ClaimDocuments.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.ClaimDocuments.Flows;

internal sealed class UploadClaimDocumentOperation
    : CreationOperation<UploadClaimDocumentOperation, UploadClaimDocumentInput, ClaimDocument>
{
    public UploadClaimDocumentOperation()
    {
        Require(input => new IsClaimAcceptingDocuments(input.Claim.State));
        Require(input => new IsFileSizeAllowed(input.Details.FileSizeBytes));
        Require(input => new IsContentTypeAllowed(input.Details.ContentType));
        Require(input => new IsContentMatchingType(input.Details.ContentType, input.Header));
    }

    protected override Outcome<ClaimDocument> Apply(UploadClaimDocumentInput input)
    {
        var document = new ClaimDocument(
            Ids.Next(),
            input.Claim.ClaimId,
            input.Claim.OrganisationId,
            input.Details,
            input.ActorId,
            input.Now);

        return Done(document);
    }
}
