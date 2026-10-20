using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.ClaimDocuments.Commands;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ClaimDocuments.Commands;

public sealed class SweepOrphanBlobsCommandTests
{
    private static readonly SweepOrphanBlobsCommand _command = new(TimeSpan.FromDays(1));

    [Fact]
    public async Task Handle_DeletesOnlyTheBlobsNoDocumentRefersTo()
    {
        var storage = CreateStorage(["claim-documents/a", "claim-documents/b", "claim-documents/c"]);
        var handler = CreateHandler(storage, CreateReader(["claim-documents/b"]));

        var result = await handler.Handle(_command, CancellationToken.None);

        result.Value.DeletedCount.Should().Be(2);
        storage.Verify(mock => mock.DeleteAsync("claim-documents/a", CancellationToken.None), Times.Once());
        storage.Verify(mock => mock.DeleteAsync("claim-documents/c", CancellationToken.None), Times.Once());
        storage.Verify(mock => mock.DeleteAsync("claim-documents/b", It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_ReferenceDifferingOnlyByCase_KeepsTheBlob()
    {
        var storage = CreateStorage(["claim-documents/file.pdf"]);
        var handler = CreateHandler(storage, CreateReader(["CLAIM-DOCUMENTS/FILE.PDF"]));

        await handler.Handle(_command, CancellationToken.None);

        storage.Verify(mock => mock.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_AsksForDocumentBlobsOlderThanTheMinimumAge()
    {
        var storage = CreateStorage([]);
        var handler = CreateHandler(storage, CreateReader([]));

        await handler.Handle(_command, CancellationToken.None);

        storage.Verify(
            mock => mock.ListAsync("claim-documents", ClaimHandlerContext.Now.AddDays(-1), CancellationToken.None),
            Times.Once());
    }

    [Fact]
    public async Task Handle_ManyBlobs_ChecksReferencesInBatches()
    {
        var paths = Enumerable.Range(0, 1_100).Select(index => $"claim-documents/{index}").ToList();
        var storage = CreateStorage(paths);
        var reader = CreateReader([]);
        var handler = CreateHandler(storage, reader);

        var result = await handler.Handle(_command, CancellationToken.None);

        result.Value.DeletedCount.Should().Be(1_100);
        reader.Verify(
            mock => mock.FindReferencedPathsAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public void Validate_WithoutAMinimumAge_FailsOnTheField()
    {
        var validator = new SweepOrphanBlobsCommandValidator();

        var result = validator.TestValidate(new SweepOrphanBlobsCommand(TimeSpan.Zero));

        result.ShouldHaveValidationErrorFor(request => request.MinimumAge);
    }

    private static Mock<IStorageService> CreateStorage(IReadOnlyList<string> paths)
    {
        var storage = new Mock<IStorageService>();
        storage
            .Setup(mock => mock.ListAsync(
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paths);
        storage
            .Setup(mock => mock.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return storage;
    }

    private static Mock<IClaimDocumentReader> CreateReader(IReadOnlyList<string> referenced)
    {
        var reader = new Mock<IClaimDocumentReader>();
        reader
            .Setup(mock => mock.FindReferencedPathsAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(referenced);

        return reader;
    }

    private static SweepOrphanBlobsCommandHandler CreateHandler(
        Mock<IStorageService> storage,
        Mock<IClaimDocumentReader> reader)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(mock => mock.UtcNow).Returns(ClaimHandlerContext.Now);

        return new SweepOrphanBlobsCommandHandler(storage.Object, reader.Object, clock.Object);
    }
}
