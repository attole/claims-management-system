using ClaimsModule.Application.ClaimDocuments;

namespace ClaimsModule.Application.Tests.ClaimDocuments;

public sealed class DocumentBlobPathTests
{
    [Fact]
    [Trait("Rule", "S:BR-D-01")]
    public void Build_UsesTheOrganisationTheClaimTheUploadIdAndTheFileName()
    {
        var organisationId = Guid.NewGuid();
        var claimId = Guid.NewGuid();
        var uploadId = Guid.NewGuid();

        var result = DocumentBlobPath.Build(organisationId, claimId, uploadId, "report.pdf");

        result.Should().Be($"claim-documents/{organisationId}/{claimId}/{uploadId:N}_report.pdf");
    }

    [Fact]
    [Trait("Rule", "S:BR-D-01")]
    public void FileNameFrom_APathBuiltForAFile_ReturnsTheOriginalName()
    {
        var path = DocumentBlobPath.Build(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "my_report.pdf");

        var result = DocumentBlobPath.FileNameFrom(path);

        result.Should().Be("my_report.pdf");
    }

    [Fact]
    public void Build_ForTwoUploadsOfTheSameName_ReturnsDifferentPaths()
    {
        var organisationId = Guid.NewGuid();
        var claimId = Guid.NewGuid();

        var first = DocumentBlobPath.Build(organisationId, claimId, Guid.NewGuid(), "report.pdf");
        var second = DocumentBlobPath.Build(organisationId, claimId, Guid.NewGuid(), "report.pdf");

        first.Should().NotBe(second);
    }

    [Fact]
    public void Build_ForTheLongestFileName_FitsTheBlobPathColumn()
    {
        var longName = new string('x', DocumentFieldLengths.DocumentName);

        var result = DocumentBlobPath.Build(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), longName);

        result.Length.Should().BeLessThanOrEqualTo(DocumentFieldLengths.BlobPath);
    }

    [Theory]
    [InlineData("report.pdf", "report.pdf")]
    [InlineData("my report (1).pdf", "my_report__1_.pdf")]
    [InlineData("..\\..\\secret.txt", "secret.txt")]
    [InlineData("a/b/c.png", "c.png")]
    [InlineData("...", "file")]
    [InlineData("", "file")]
    public void Sanitise_KeepsOnlySafeCharactersOfTheLastSegment(string fileName, string expected)
    {
        var result = DocumentBlobPath.Sanitise(fileName);

        result.Should().Be(expected);
    }
}
