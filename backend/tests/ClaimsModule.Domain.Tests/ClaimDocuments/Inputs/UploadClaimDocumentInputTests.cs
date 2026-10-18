using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Tests.Support.ClaimDocuments;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.ClaimDocuments.Inputs;

public sealed class UploadClaimDocumentInputTests
{
    [Fact]
    public void Now_WhenNotGiven_Throws()
    {
        var claim = TestDocuments.Claim(new ClaimOpenState());
        Func<UploadClaimDocumentInput> act = () => new UploadClaimDocumentInput(
            claim,
            TestDocuments.Details(),
            TestClaims.Actor,
            default);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ActorId_WhenNotGiven_Throws()
    {
        var claim = TestDocuments.Claim(new ClaimOpenState());
        Func<UploadClaimDocumentInput> act = () => new UploadClaimDocumentInput(
            claim,
            TestDocuments.Details(),
            Guid.Empty,
            TestClaims.Start);

        act.Should().Throw<ArgumentException>();
    }
}
