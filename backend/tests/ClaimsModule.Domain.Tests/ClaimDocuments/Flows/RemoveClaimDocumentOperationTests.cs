using ClaimsModule.Domain.ClaimDocuments.Flows;
using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.ClaimDocuments.Rules;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Tests.Support.ClaimDocuments;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.ClaimDocuments.Flows;

public sealed class RemoveClaimDocumentOperationTests
{
    [Fact]
    public void Run_MarksTheDocumentDeletedAtTheGivenTime()
    {
        var document = TestDocuments.Upload();
        var input = new RemoveClaimDocumentInput(new ClaimOpenState(), TestClaims.Later);

        var outcome = RemoveClaimDocumentOperation.Run(document, input);

        outcome.IsSuccess.Should().BeTrue();
        document.IsDeleted.Should().BeTrue();
        document.DeletedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", IsClaimAcceptingDocuments.Code)]
    public void Run_OnAClosedClaim_ReturnsTheFindingAndKeepsTheDocument()
    {
        var document = TestDocuments.Upload();
        var input = new RemoveClaimDocumentInput(new ClaimClosedState(), TestClaims.Later);

        var outcome = RemoveClaimDocumentOperation.Run(document, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimAcceptingDocuments.Code);
        document.IsDeleted.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", IsClaimDocumentNotRemoved.Code)]
    public void Run_ForARemovedDocument_ReturnsTheFinding()
    {
        var document = TestDocuments.Upload();
        var input = new RemoveClaimDocumentInput(new ClaimOpenState(), TestClaims.Later);
        RemoveClaimDocumentOperation.Run(document, input);

        var outcome = RemoveClaimDocumentOperation.Run(document, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimDocumentNotRemoved.Code);
    }

    [Fact]
    public void Run_WithATimeBeforeTheUpload_Throws()
    {
        var document = TestDocuments.Upload();
        var input = new RemoveClaimDocumentInput(new ClaimOpenState(), TestClaims.Start.AddHours(-1));

        Action act = () => RemoveClaimDocumentOperation.Run(document, input);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
