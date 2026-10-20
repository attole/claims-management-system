using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ClaimsModule.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Storage.Local;

public sealed class LocalDownloadToken(IOptions<StorageOptions> options, IClock clock)
{
    public const string DownloadRoute = "/api/documents/download";

    private const char Separator = '.';
    private const char FieldSeparator = '|';

    public string Create(string path, TimeSpan lifetime)
    {
        var expiresAt = clock.UtcNow.Add(lifetime).ToUnixTimeSeconds();
        var payload = Base64Url.EncodeToString(Encoding.UTF8.GetBytes($"{expiresAt}{FieldSeparator}{path}"));

        return $"{payload}{Separator}{Sign(payload)}";
    }

    public string ReadPath(string token)
    {
        if (token.Split(Separator) is not [var payload, var signature])
        {
            return string.Empty;
        }

        var provided = Encoding.UTF8.GetBytes(signature);
        var expected = Encoding.UTF8.GetBytes(Sign(payload));

        if (!CryptographicOperations.FixedTimeEquals(provided, expected))
        {
            return string.Empty;
        }

        var fields = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(payload)).Split(FieldSeparator, 2);
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(fields[0], CultureInfo.InvariantCulture));

        return expiresAt > clock.UtcNow ? fields[1] : string.Empty;
    }

    private string Sign(string payload)
    {
        var key = Encoding.UTF8.GetBytes(options.Value.LocalSigningKey);
        var data = Encoding.UTF8.GetBytes(payload);

        return Base64Url.EncodeToString(HMACSHA256.HashData(key, data));
    }
}
