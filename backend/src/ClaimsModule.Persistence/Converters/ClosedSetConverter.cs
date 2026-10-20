using System.Linq.Expressions;
using ClaimsModule.Domain.Common.Markers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Converters;

internal sealed class ClosedSetConverter<TSet, TKind>(Func<TKind, TSet> from)
    : ValueConverter<TSet, string>(_toColumn, FromColumn(from))
    where TSet : Marked<TKind>
    where TKind : struct, Enum
{
    private static readonly Expression<Func<TSet, string>> _toColumn = set => set.Name;

    private static Expression<Func<string, TSet>> FromColumn(Func<TKind, TSet> from)
    {
        return text => from(Enum.Parse<TKind>(text));
    }
}
