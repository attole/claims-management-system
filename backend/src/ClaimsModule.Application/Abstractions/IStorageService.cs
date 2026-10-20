namespace ClaimsModule.Application.Abstractions;

public interface IStorageService
{
    Task UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken);

    Task<string> GetDownloadUrlAsync(string path, TimeSpan lifetime, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListAsync(
        string container,
        DateTimeOffset createdBefore,
        CancellationToken cancellationToken);

    Task DeleteAsync(string path, CancellationToken cancellationToken);
}
