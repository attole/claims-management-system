namespace ClaimsModule.Application.Abstractions;

public interface IStorageService
{
    Task UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken);

    Task<string> GetDownloadUrlAsync(string path, TimeSpan lifetime, CancellationToken cancellationToken);
}
