namespace ClaimsModule.Infrastructure.Storage;

internal readonly record struct StoragePath(string Container, string Name)
{
    public static StoragePath From(string path)
    {
        var separator = path.IndexOf('/', StringComparison.Ordinal);

        if (separator <= 0)
        {
            throw new ArgumentException("A storage path is <container>/<name>.", nameof(path));
        }

        return new StoragePath(path[..separator], path[(separator + 1)..]);
    }
}
