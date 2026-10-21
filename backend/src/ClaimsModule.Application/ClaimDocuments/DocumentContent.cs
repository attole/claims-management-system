using ClaimsModule.Domain.ClaimDocuments.Inputs;

namespace ClaimsModule.Application.ClaimDocuments;

public sealed record DocumentContent(byte[] Header, Stream Content)
{
    public static async Task<DocumentContent> ReadAsync(Stream content, CancellationToken cancellationToken)
    {
        var seekable = content.CanSeek ? content : await BufferAsync(content, cancellationToken);
        var header = new byte[UploadClaimDocumentInput.HeaderLength];
        int read = await seekable.ReadAtLeastAsync(header, header.Length, false, cancellationToken);
        seekable.Position = 0;

        return new DocumentContent(header[..read], seekable);
    }

    private static async Task<Stream> BufferAsync(Stream content, CancellationToken cancellationToken)
    {
        var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        return buffer;
    }
}
