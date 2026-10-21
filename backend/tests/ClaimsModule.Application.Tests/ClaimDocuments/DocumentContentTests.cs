using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments.Inputs;

namespace ClaimsModule.Application.Tests.ClaimDocuments;

public sealed class DocumentContentTests
{
    [Fact]
    public async Task ReadAsync_ForASeekableStream_ReturnsTheHeaderAndTheStreamFromTheStart()
    {
        var bytes = Enumerable.Range(0, 1000).Select(value => (byte)value).ToArray();
        var stream = new MemoryStream(bytes);

        var result = await DocumentContent.ReadAsync(stream, CancellationToken.None);

        result.Header.Should().Equal(bytes.Take(UploadClaimDocumentInput.HeaderLength));
        result.Content.Should().BeSameAs(stream);
        result.Content.Position.Should().Be(0);
    }

    [Fact]
    public async Task ReadAsync_ForAShortStream_ReturnsWhatThereIs()
    {
        var stream = new MemoryStream([1, 2, 3]);

        var result = await DocumentContent.ReadAsync(stream, CancellationToken.None);

        result.Header.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task ReadAsync_ForAForwardOnlyStream_BuffersItSoTheWholeContentCanStillBeStored()
    {
        var bytes = "%PDF-1.7 forward only"u8.ToArray();
        using var forwardOnly = new ForwardOnlyStream(bytes);

        var result = await DocumentContent.ReadAsync(forwardOnly, CancellationToken.None);

        result.Header.Should().Equal(bytes);
        ((MemoryStream)result.Content).ToArray().Should().Equal(bytes);
    }

    private sealed class ForwardOnlyStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
    }
}
