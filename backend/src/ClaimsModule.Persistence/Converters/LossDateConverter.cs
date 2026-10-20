using System.Linq.Expressions;
using ClaimsModule.Domain.Claims.Loss;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Converters;

internal sealed class LossDateConverter() : ValueConverter<LossDate, DateTimeOffset?>(_toColumn, _fromColumn)
{
    private static readonly Expression<Func<LossDate, DateTimeOffset?>> _toColumn =
        date => date.IsSet ? date.Value : null;

    private static readonly Expression<Func<DateTimeOffset?, LossDate>> _fromColumn =
        value => value.HasValue ? new LossDate(value.Value) : LossDate.Empty;

    public override bool ConvertsNulls => true;
}
