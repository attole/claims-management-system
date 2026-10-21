using ClaimsModule.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Storage.Local;

public sealed class LocalFileSystemStorageService(IOptions<StorageOptions> options, LocalDownloadToken tokens)
    : IStorageService
{
    public async Task UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var file = Resolve(path);
        Directory.CreateDirectory(Path.GetDirectoryName(file) ?? string.Empty);

        await using var output = File.Create(file);
        await content.CopyToAsync(output, cancellationToken);
    }

    public Task<string> GetDownloadUrlAsync(string path, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        var token = tokens.Create(path, lifetime);
        var baseUrl = options.Value.LocalBaseUrl.TrimEnd('/');
        var url = $"{baseUrl}{LocalDownloadToken.DownloadRoute}?token={Uri.EscapeDataString(token)}";

        return Task.FromResult(url);
    }

    public Task<IReadOnlyList<string>> ListAsync(
        string container,
        DateTimeOffset createdBefore,
        CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(options.Value.LocalRoot);
        var files = Directory.Exists(root)
            ? Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            : [];
        var paths = files
            .Where(file => File.GetLastWriteTimeUtc(file) < createdBefore.UtcDateTime)
            .Select(file => $"{container}/{Path.GetRelativePath(root, file).Replace('\\', '/')}")
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(paths);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        var file = Resolve(path);

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        return Task.CompletedTask;
    }

    public bool HasFile(string path)
    {
        return File.Exists(Resolve(path));
    }

    public Stream OpenRead(string path)
    {
        return File.OpenRead(Resolve(path));
    }

    private string Resolve(string path)
    {
        var root = Path.GetFullPath(options.Value.LocalRoot);
        var file = Path.GetFullPath(Path.Combine(root, StoragePath.From(path).Name));

        if (!file.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ArgumentException("The path leaves the storage root.", nameof(path));
        }

        return file;
    }
}
