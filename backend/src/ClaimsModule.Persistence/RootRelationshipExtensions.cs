using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence;

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

    public static ReferenceReferenceBuilder<TRoot, TRow> MarksRootModified<TRoot, TRow>(
        this ReferenceReferenceBuilder<TRoot, TRow> builder)
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
