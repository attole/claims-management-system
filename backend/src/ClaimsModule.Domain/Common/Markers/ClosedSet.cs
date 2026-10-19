using System.Collections.Frozen;

namespace ClaimsModule.Domain.Common.Markers;

internal static class ClosedSet<TSet, TKind>
    where TSet : Marked<TKind>
    where TKind : struct, Enum
{
    private static readonly FrozenDictionary<TKind, TSet> _variants = FindVariants();

    public static TSet From(TKind kind)
    {
        if (_variants.TryGetValue(kind, out var variant))
        {
            return variant;
        }

        throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown kind.");
    }

    private static FrozenDictionary<TKind, TSet> FindVariants()
    {
        var variants = typeof(TSet).Assembly
            .GetTypes()
            .Where(type => type.IsSealed)
            .Where(type => type.IsSubclassOf(typeof(TSet)))
            .Select(type => (TSet)Activator.CreateInstance(type)!)
            .ToFrozenDictionary(variant => variant.Marker);

        return variants;
    }
}
