namespace ClaimsModule.Domain.ClaimDocuments;

internal static class DocumentContentTypes
{
    private static readonly byte[] _pdf = "%PDF"u8.ToArray();
    private static readonly byte[] _png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] _jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] _zip = [0x50, 0x4B, 0x03, 0x04];

    private static readonly Dictionary<string, Func<IReadOnlyList<byte>, bool>> _signatures =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = header => HasSignature(header, _pdf),
            ["image/jpeg"] = header => HasSignature(header, _jpeg),
            ["image/png"] = header => HasSignature(header, _png),
            ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] =
                header => HasSignature(header, _zip),
            ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] =
                header => HasSignature(header, _zip),
            ["text/plain"] = IsText,
            ["text/csv"] = IsText,
        };

    public static bool IsAllowed(string contentType)
    {
        return _signatures.ContainsKey(contentType);
    }

    public static bool IsMatching(string contentType, IReadOnlyList<byte> header)
    {
        return !_signatures.TryGetValue(contentType, out var matches) || matches(header);
    }

    private static bool HasSignature(IReadOnlyList<byte> header, byte[] signature)
    {
        return header.Count >= signature.Length && header.Take(signature.Length).SequenceEqual(signature);
    }

    private static bool IsText(IReadOnlyList<byte> header)
    {
        return !header.Contains((byte)0);
    }
}
