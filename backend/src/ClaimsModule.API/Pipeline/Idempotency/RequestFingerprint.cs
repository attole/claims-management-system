using System.Security.Cryptography;
using System.Text;

namespace ClaimsModule.API.Pipeline.Idempotency;

public static class RequestFingerprint
{
    private const int BufferSize = 81920;

    public static Task<string> ComputeAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        request.EnableBuffering();

        return request.HasFormContentType
            ? HashFormAsync(request, cancellationToken)
            : HashBodyAsync(request, cancellationToken);
    }

    private static async Task<string> HashBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var hash = await SHA256.HashDataAsync(request.Body, cancellationToken);
        request.Body.Position = 0;

        return Convert.ToHexString(hash);
    }

    private static async Task<string> HashFormAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var form = await request.ReadFormAsync(cancellationToken);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        Append(hash, form.Count.ToString());

        foreach (var field in form)
        {
            Append(hash, field.Key);
            var values = field.Value.Select(value => value ?? string.Empty);
            Append(hash, string.Concat(values.Select(value => $"{value.Length}:{value}")));
        }

        Append(hash, form.Files.Count.ToString());

        foreach (var file in form.Files)
        {
            Append(hash, file.Name);
            Append(hash, file.FileName);
            Append(hash, file.Length.ToString());

            await using var stream = file.OpenReadStream();
            var buffer = new byte[BufferSize];
            int read;

            while ((read = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                hash.AppendData(buffer, 0, read);
            }
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static void Append(IncrementalHash hash, string value)
    {
        hash.AppendData(Encoding.UTF8.GetBytes($"{value.Length}:{value}"));
    }
}
