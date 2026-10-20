using System.Security.Cryptography;
using System.Text;

namespace ClaimsModule.Persistence.Schema.Seed;

internal static class SeedIds
{
    public static readonly Guid Organisation = For("organisation");

    public static readonly Guid SystemUser = For("system-user");

    public static readonly DateTimeOffset SeededAt = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static Guid For(string name)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(name));

        return new Guid(hash);
    }
}
