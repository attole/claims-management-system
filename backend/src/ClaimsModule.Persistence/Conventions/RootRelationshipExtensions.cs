using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Conventions;

internal static class RootRelationshipExtensions
{
    private const string MarkerName = "Claims:MarksRootModified";

    public static ReferenceCollectionBuilder<TRoot, TRow> MarksRootModified<TRoot, TRow>(
        this ReferenceCollectionBuilder<TRoot, TRow> builder)
        where TRoot : class
        where TRow : class
    {
        builder.Metadata.SetAnnotation(MarkerName, true);

        return builder;
    }

    public static bool IsMarkedToModifyRoot(this IReadOnlyForeignKey key)
    {
        bool isMarked = key.FindAnnotation(MarkerName) is not null;

        return isMarked;
    }
}
