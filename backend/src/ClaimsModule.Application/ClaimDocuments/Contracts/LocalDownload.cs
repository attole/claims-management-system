namespace ClaimsModule.Application.ClaimDocuments.Contracts;

public sealed record LocalDownload(Stream Content, string FileName, string ContentType)
{
    public static LocalDownload Empty { get; } = new(Stream.Null, string.Empty, string.Empty);

    public bool IsFound => FileName.Length > 0;
}
