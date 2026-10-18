using ClaimsModule.Domain.ClaimDocuments.Flows;
using ClaimsModule.Domain.ClaimDocuments.Rules;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Tests.Support.ClaimDocuments;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.ClaimDocuments.Flows;

public sealed class UploadClaimDocumentOperationTests
{
    [Fact]
    public void Run_CreatesTheDocumentWithTheDetailsTheUploaderAndTheTime()
    {
        var input = TestDocuments.UploadInput();

        var outcome = UploadClaimDocumentOperation.Run(input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Details.Should().Be(input.Details);
        outcome.Value.UploadedByUserId.Should().Be(TestClaims.Actor);
        outcome.Value.UploadedAt.Should().Be(TestClaims.Start);
        outcome.Value.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Run_TakesTheClaimAndTheOrganisationFromTheClaimReference()
    {
        var input = TestDocuments.UploadInput();

        var outcome = UploadClaimDocumentOperation.Run(input);

        outcome.Value.ClaimId.Should().Be(TestDocuments.ClaimId);
        outcome.Value.OrganisationId.Should().Be(TestClaims.Organisation);
    }

    [Fact]
    [Trait("Rule", IsClaimAcceptingDocuments.Code)]
    public void Run_OnAClosedClaim_ReturnsTheFinding()
    {
        var claim = TestDocuments.Claim(new ClaimClosedState());
        var input = TestDocuments.UploadInput() with { Claim = claim };

        var outcome = UploadClaimDocumentOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimAcceptingDocuments.Code);
    }

    [Fact]
    [Trait("Rule", IsFileSizeAllowed.Code)]
    public void Run_WithAFileOverTheLimit_ReturnsTheFinding()
    {
        var details = TestDocuments.Details() with { FileSizeBytes = IsFileSizeAllowed.MaxBytes + 1 };
        var input = TestDocuments.UploadInput() with { Details = details };

        var outcome = UploadClaimDocumentOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsFileSizeAllowed.Code);
    }

    [Fact]
    [Trait("Rule", IsContentTypeAllowed.Code)]
    public void Run_WithATypeOutsideTheAllowlist_ReturnsTheFinding()
    {
        var details = TestDocuments.Details() with { ContentType = "application/x-msdownload" };
        var input = TestDocuments.UploadInput() with { Details = details };

        var outcome = UploadClaimDocumentOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsContentTypeAllowed.Code);
    }
}
