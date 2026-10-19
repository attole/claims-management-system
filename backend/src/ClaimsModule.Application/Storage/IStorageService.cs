namespace ClaimsModule.Application.Storage;

public interface IStorageService
{
    Task UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken);
}
