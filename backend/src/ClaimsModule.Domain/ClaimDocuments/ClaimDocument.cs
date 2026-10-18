using ClaimsModule.Domain.ClaimDocuments.Events;
using ClaimsModule.Domain.ClaimDocuments.Flows;
using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.ClaimDocuments;

public sealed class ClaimDocument : AggregateRoot, ISoftDeletable
{
    private ClaimDocument()
    {
        Details = new ClaimDocumentDetails(string.Empty, string.Empty, string.Empty, string.Empty, 0);
    }

    internal ClaimDocument(
        Guid id,
        Guid claimId,
        Guid organisationId,
        ClaimDocumentDetails details,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAt)
        : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        Details = details;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = uploadedAt;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public ClaimDocumentDetails Details { get; private set; }

    public Guid UploadedByUserId { get; private set; } = Guid.Empty;

    public DateTimeOffset UploadedAt { get; private set; } = DateTimeOffset.MinValue;

    public bool IsDeleted { get; internal set; } = false;

    public DateTimeOffset DeletedAt { get; internal set; } = DateTimeOffset.MinValue;

    public static Outcome<ClaimDocument> Upload(UploadClaimDocumentInput input)
    {
        return UploadClaimDocumentOperation.Run(input)
            .OnSuccess(document => document.RecordUpload());
    }

    public Outcome<ClaimDocument> Remove(RemoveClaimDocumentInput input)
    {
        return RemoveClaimDocumentOperation.Run(this, input)
            .OnSuccess(() => Raise(new ClaimDocumentRemovedEvent(Id, ClaimId)));
    }

    private void RecordUpload()
    {
        var uploaded = new ClaimDocumentUploadedEvent(Id, ClaimId, Details.DocumentType, Details.DocumentName);
        Raise(uploaded);
    }
}
