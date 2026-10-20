using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments.Events;
using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.ClaimDocuments.Rules;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Tests.Support.ClaimDocuments;

namespace ClaimsModule.Domain.Tests.ClaimDocuments;

public sealed class ClaimDocumentTests
{
    [Fact]
    [Trait("Rule", "S:DOC-Audit")]
    public void Upload_RaisesOneUploadedEvent()
    {
        var input = TestDocuments.UploadInput();

        var outcome = ClaimDocument.Upload(input);

        var uploaded = outcome.Value.PopEvents().OfType<ClaimDocumentUploadedEvent>().Should().ContainSingle().Subject;
        uploaded.ClaimDocumentId.Should().Be(outcome.Value.Id);
        uploaded.ClaimId.Should().Be(TestDocuments.ClaimId);
        uploaded.DocumentType.Should().Be("PoliceReport");
        uploaded.DocumentName.Should().Be("report.pdf");
    }

    [Fact]
    [Trait("Rule", IsClaimAcceptingDocuments.Code)]
    public void Upload_OnAFrozenClaim_ReturnsTheFindingAndRaisesNothing()
    {
        var claim = TestDocuments.Claim(new ClaimClosedState());
        var input = TestDocuments.UploadInput() with { Claim = claim };

        var outcome = ClaimDocument.Upload(input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimAcceptingDocuments.Code);
    }

    [Fact]
    [Trait("Rule", "S:DOC-Audit")]
    public void Remove_RaisesOneRemovedEvent()
    {
        var document = TestDocuments.Upload();
        var input = new RemoveClaimDocumentInput(new ClaimOpenState());

        document.Remove(input);

        var removed = document.PopEvents().OfType<ClaimDocumentRemovedEvent>().Should().ContainSingle().Subject;
        removed.ClaimDocumentId.Should().Be(document.Id);
        removed.ClaimId.Should().Be(TestDocuments.ClaimId);
    }

    [Fact]
    [Trait("Rule", IsClaimDocumentNotRemoved.Code)]
    public void Remove_ARemovedDocument_ReturnsTheFindingAndRaisesNothing()
    {
        var document = TestDocuments.Upload();
        var input = new RemoveClaimDocumentInput(new ClaimOpenState());
        document.Remove(input);
        document.PopEvents();

        var outcome = document.Remove(input);

        outcome.IsSuccess.Should().BeFalse();
        document.PopEvents().Should().BeEmpty();
    }
}
